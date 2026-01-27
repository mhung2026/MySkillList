"""
AI Question Generation API V2
Endpoints for generating questions and grading answers with new schema
"""

from fastapi import APIRouter, HTTPException, status
from pydantic import BaseModel, Field
from typing import List, Optional, Dict, Any
from datetime import datetime
import logging

from ..validators.request_validator import validate_and_normalize, RequestValidator
from db_skill_reader import (
    getDistinctSkillsWithLevels,
    getSkillLevelsBySkillId,
    getSkillLevelCount
)
from ..generators.question_generator_v2 import generate_questions_v2 as ai_generate_questions
from ..generators.answer_grader import grade_answer as ai_grade_answer
from ..generators.skill_gap_analyzer import analyze_skill_gap, analyze_multiple_gaps
from ..generators.learning_path_recommender import generate_learning_path, rank_learning_resources

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/v2", tags=["AI Generation V2"])

# Request Models
class SkillInfo(BaseModel):
    skill_id: str
    skill_name: str
    skill_code: Optional[str] = None

class GenerateRequestV2(BaseModel):
    question_type: List[str] = Field(..., min_items=1, description="Question types to generate")
    language: str = Field(..., description="Language: 'English' or 'Vietnamese'")
    number_of_questions: int = Field(..., ge=1, le=100, description="Number of questions")
    skills: Optional[List[SkillInfo]] = Field(None, description="Skills to focus on (optional)")
    target_proficiency_level: Optional[List[int]] = Field(None, description="SFIA levels 1-7 (optional)")
    difficulty: Optional[str] = Field(None, description="Difficulty: Easy/Medium/Hard (optional)")
    additional_context: Optional[str] = Field(None, max_length=2000, description="Additional context")

class GradeAnswerRequest(BaseModel):
    """Request to grade a student's answer."""
    question_id: Optional[str] = Field(None, description="Question ID (optional)")
    question_content: str = Field(..., description="The question text")
    student_answer: str = Field(..., description="Student's submitted answer")
    max_points: int = Field(..., ge=1, le=100, description="Maximum points for this question")
    grading_rubric: Optional[str] = Field(None, description="JSON string with grading criteria")
    expected_answer: Optional[str] = Field(None, description="Expected/model answer")
    question_type: Optional[str] = Field(None, description="Question type (ShortAnswer, LongAnswer, etc.)")
    language: Optional[str] = Field("en", description="Response language (en/vi)")

class GradeAnswerResponse(BaseModel):
    """Response from grading an answer."""
    success: bool
    points_awarded: int
    max_points: int
    percentage: float
    feedback: str
    strength_points: List[str]
    improvement_areas: List[str]
    detailed_analysis: Optional[str] = None

# Response Models
class SkillResponse(BaseModel):
    skill_id: str
    skill_name: str
    skill_code: str
    level_count: int

class SkillLevelResponse(BaseModel):
    level: int
    description: str
    autonomy: Optional[str]
    influence: Optional[str]
    complexity: Optional[str]

# Endpoints
@router.get("/skills", response_model=Dict[str, Any])
async def get_all_skills():
    """
    Get all available skills from database.
    Returns list of skills with their level counts.
    """
    try:
        logger.info("Fetching all skills from database")
        skills = getDistinctSkillsWithLevels()

        skill_list = [
            {
                "skill_id": str(s[0]),
                "skill_name": s[1],
                "skill_code": s[2],
                "level_count": s[3]
            }
            for s in skills
        ]

        logger.info(f"Retrieved {len(skill_list)} skills")
        return {
            "success": True,
            "skills": skill_list,
            "total": len(skill_list)
        }
    except Exception as e:
        logger.error(f"Error fetching skills: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to fetch skills: {str(e)}"
        )

@router.get("/skills/{skill_id}/levels")
async def get_skill_levels(skill_id: str):
    """
    Get proficiency levels for a specific skill.
    Returns all level definitions (1-7) for the skill.
    """
    try:
        logger.info(f"Fetching levels for skill: {skill_id}")
        levels = getSkillLevelsBySkillId(skill_id)

        if not levels:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"No levels found for skill: {skill_id}"
            )

        level_list = [
            {
                "level": l[0],
                "description": l[1],
                "autonomy": l[2],
                "influence": l[3],
                "complexity": l[4],
                "business_skills": l[5],
                "knowledge": l[6]
            }
            for l in levels
        ]

        logger.info(f"Retrieved {len(level_list)} levels for skill {skill_id}")
        return {
            "success": True,
            "skill_id": skill_id,
            "levels": level_list
        }
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error fetching skill levels: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to fetch skill levels: {str(e)}"
        )

@router.get("/stats")
async def get_database_stats():
    """Get database statistics."""
    try:
        total_definitions = getSkillLevelCount()
        skills = getDistinctSkillsWithLevels()

        return {
            "success": True,
            "stats": {
                "total_skills": len(skills),
                "total_level_definitions": total_definitions,
                "sfia_levels": "1-7"
            }
        }
    except Exception as e:
        logger.error(f"Error fetching stats: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )

@router.post("/generate-questions")
async def generate_questions_v2(request: GenerateRequestV2):
    """
    Generate questions using V2 schema.

    This endpoint:
    1. Validates the request
    2. Fetches skill data from database
    3. Generates questions using AI (Azure OpenAI)
    4. Returns questions in output_question_schema_v2 format
    """
    try:
        logger.info(f"Received generation request: {request.number_of_questions} questions")
        logger.debug(f"Request details: {request.dict()}")

        # 1. Validate and normalize request
        request_dict = request.dict()
        is_valid, error, normalized = validate_and_normalize(request_dict)

        if not is_valid:
            logger.error(f"Validation failed: {error}")
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail=f"Invalid request: {error}"
            )

        logger.info("Request validated successfully")
        logger.debug(f"Normalized request: {normalized}")

        # 2. Fetch skill data from DB if provided
        skill_data = None
        if normalized.get("skills") and len(normalized["skills"]) > 0:
            skill_id = normalized["skills"][0]["skill_id"]
            skill_name = normalized["skills"][0]["skill_name"]
            skill_code = normalized["skills"][0].get("skill_code", "")

            logger.info(f"Fetching skill data for: {skill_name} ({skill_code}) - {skill_id}")
            levels = getSkillLevelsBySkillId(skill_id)

            if not levels:
                logger.warning(f"No levels found for skill {skill_id}, proceeding without skill data")
            else:
                logger.info(f"Retrieved {len(levels)} levels for skill")
                # Format skill data for AI generator
                # Fields from DB: Level, Description, Autonomy, Influence, Complexity, BusinessSkills, Knowledge, BehavioralIndicators, EvidenceExamples
                skill_data = {
                    "skill_id": skill_id,
                    "skill_name": skill_name,
                    "skill_code": skill_code,
                    "levels": [
                        {
                            "level": l[0],
                            "description": l[1],
                            "autonomy": l[2],
                            "influence": l[3],
                            "complexity": l[4],
                            "business_skills": l[5],
                            "knowledge": l[6],
                            "behavioral_indicators": l[7],
                            "evidence_examples": l[8]
                        }
                        for l in levels
                    ]
                }

        # 3. Generate questions with Azure OpenAI
        logger.info("Generating questions with Azure OpenAI")

        try:
            result = await ai_generate_questions(normalized, skill_data)
            logger.info(f"Successfully generated {result['metadata']['total_questions']} questions with AI")
            return result
        except ValueError as e:
            logger.error(f"AI generation failed: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"AI generation failed: {str(e)}"
            )
        except Exception as e:
            logger.error(f"Unexpected error during AI generation: {e}", exc_info=True)
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Unexpected error during AI generation: {str(e)}"
            )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error generating questions: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to generate questions: {str(e)}"
        )

@router.get("/health")
async def health_check():
    """Health check for V2 API."""
    try:
        # Test DB connection
        count = getSkillLevelCount()

        return {
            "status": "healthy",
            "api_version": "v2",
            "database": "connected",
            "total_definitions": count
        }
    except Exception as e:
        return {
            "status": "unhealthy",
            "api_version": "v2",
            "database": "disconnected",
            "error": str(e)
        }

@router.post("/grade-answer", response_model=GradeAnswerResponse)
async def grade_answer_endpoint(request: GradeAnswerRequest):
    """
    Grade a student's answer using AI.

    This endpoint:
    1. Takes the question, student answer, and grading criteria
    2. Uses Azure OpenAI to evaluate the answer
    3. Returns points, feedback, strengths, and improvement areas
    """
    try:
        logger.info(f"Grading answer for question (max_points={request.max_points})")
        logger.debug(f"Question: {request.question_content[:100]}...")
        logger.debug(f"Student answer length: {len(request.student_answer)} chars")

        # Call AI grader
        result = await ai_grade_answer(
            question_content=request.question_content,
            student_answer=request.student_answer,
            max_points=request.max_points,
            grading_rubric=request.grading_rubric,
            expected_answer=request.expected_answer,
            question_type=request.question_type,
            language=request.language or "en"
        )

        logger.info(f"Grading complete: {result['points_awarded']}/{result['max_points']} ({result['percentage']}%)")
        return GradeAnswerResponse(**result)

    except ValueError as e:
        logger.error(f"Grading failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Grading failed: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during grading: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Unexpected error: {str(e)}"
        )


# ============================================================================
# SKILL GAP ANALYSIS ENDPOINTS
# ============================================================================

class SkillGapRequest(BaseModel):
    """Request to analyze a single skill gap."""
    employee_name: str = Field(..., description="Employee name")
    job_role: str = Field(..., description="Job role name")
    skill_id: Optional[str] = Field(None, description="Skill ID")
    skill_name: str = Field(..., description="Skill name")
    skill_code: str = Field(..., description="Skill code")
    current_level: int = Field(..., ge=0, le=7, description="Current proficiency level (0-7)")
    required_level: int = Field(..., ge=1, le=7, description="Required proficiency level (1-7)")
    skill_description: Optional[str] = Field(None, description="Skill description")
    current_level_description: Optional[str] = Field(None, description="Current level description")
    required_level_description: Optional[str] = Field(None, description="Required level description")
    language: Optional[str] = Field("en", description="Response language (en/vi)")


class SkillGapResponse(BaseModel):
    """Response from skill gap analysis."""
    success: bool
    ai_analysis: str
    ai_recommendation: str
    priority_rationale: str
    estimated_effort: str
    key_actions: List[str]
    potential_blockers: List[str]


class MultipleGapsRequest(BaseModel):
    """Request to analyze multiple skill gaps."""
    employee_name: str = Field(..., description="Employee name")
    job_role: str = Field(..., description="Job role name")
    gaps: List[Dict[str, Any]] = Field(..., description="List of gaps to analyze")
    language: Optional[str] = Field("en", description="Response language (en/vi)")


class MultipleGapsResponse(BaseModel):
    """Response from multiple gaps analysis."""
    success: bool
    gap_analyses: List[Dict[str, Any]]
    overall_summary: str
    priority_order: List[str]
    recommended_focus_areas: List[str]


@router.post("/analyze-gap", response_model=SkillGapResponse)
async def analyze_gap_endpoint(request: SkillGapRequest):
    """
    Analyze a skill gap using AI.

    This endpoint:
    1. Takes skill gap details (employee, skill, current/required levels)
    2. Uses Azure OpenAI to analyze the gap
    3. Returns AI-generated analysis, recommendations, and action items
    """
    try:
        logger.info(f"Analyzing gap for {request.skill_name}: {request.current_level} -> {request.required_level}")

        result = await analyze_skill_gap(
            employee_name=request.employee_name,
            job_role=request.job_role,
            skill_name=request.skill_name,
            skill_code=request.skill_code,
            current_level=request.current_level,
            required_level=request.required_level,
            skill_description=request.skill_description,
            current_level_description=request.current_level_description,
            required_level_description=request.required_level_description,
            language=request.language or "en"
        )

        logger.info(f"Gap analysis complete for {request.skill_name}")
        return SkillGapResponse(**result)

    except ValueError as e:
        logger.error(f"Gap analysis failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Gap analysis failed: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during gap analysis: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Unexpected error: {str(e)}"
        )


@router.post("/analyze-gaps", response_model=MultipleGapsResponse)
async def analyze_multiple_gaps_endpoint(request: MultipleGapsRequest):
    """
    Analyze multiple skill gaps using AI.

    This endpoint:
    1. Takes a list of skill gaps
    2. Analyzes each gap using Azure OpenAI
    3. Returns individual analyses plus overall summary and prioritization
    """
    try:
        logger.info(f"Analyzing {len(request.gaps)} gaps for {request.employee_name}")

        result = await analyze_multiple_gaps(
            employee_name=request.employee_name,
            job_role=request.job_role,
            gaps=request.gaps,
            language=request.language or "en"
        )

        logger.info(f"Multiple gaps analysis complete: {len(result['gap_analyses'])} analyzed")
        return MultipleGapsResponse(**result)

    except ValueError as e:
        logger.error(f"Multiple gaps analysis failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Analysis failed: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during multiple gaps analysis: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Unexpected error: {str(e)}"
        )


# ============================================================================
# LEARNING PATH ENDPOINTS
# ============================================================================

class LearningResourceInfo(BaseModel):
    """Learning resource info for ranking."""
    id: str
    title: str
    type: str
    description: Optional[str] = None
    estimated_hours: Optional[int] = None
    difficulty: Optional[str] = None
    from_level: Optional[int] = None
    to_level: Optional[int] = None


class GenerateLearningPathRequest(BaseModel):
    """Request to generate a learning path."""
    employee_name: str = Field(..., description="Employee name")
    skill_id: Optional[str] = Field(None, description="Skill ID")
    skill_name: str = Field(..., description="Skill name")
    skill_code: str = Field(..., description="Skill code")
    current_level: int = Field(..., ge=0, le=7, description="Current proficiency level (0-7)")
    target_level: int = Field(..., ge=1, le=7, description="Target proficiency level (1-7)")
    skill_description: Optional[str] = Field(None, description="Skill description")
    available_resources: Optional[List[LearningResourceInfo]] = Field(None, description="Available learning resources")
    time_constraint_months: Optional[int] = Field(None, ge=1, le=24, description="Time constraint in months")
    language: Optional[str] = Field("en", description="Response language (en/vi)")


class LearningPathItemResponse(BaseModel):
    """Learning path item in response."""
    order: int
    title: str
    description: str
    item_type: str
    estimated_hours: int
    target_level_after: int
    success_criteria: str
    resource_id: Optional[str] = None


class MilestoneResponse(BaseModel):
    """Milestone in response."""
    after_item: int
    description: str
    expected_level: int


class GenerateLearningPathResponse(BaseModel):
    """Response from learning path generation."""
    success: bool
    path_title: str
    path_description: str
    estimated_total_hours: int
    estimated_duration_weeks: int
    learning_items: List[Dict[str, Any]]
    milestones: List[Dict[str, Any]]
    ai_rationale: str
    key_success_factors: List[str]
    potential_challenges: List[str]


class RankResourcesRequest(BaseModel):
    """Request to rank learning resources."""
    skill_name: str = Field(..., description="Skill name")
    skill_code: str = Field(..., description="Skill code")
    current_level: int = Field(..., ge=0, le=7, description="Current level")
    target_level: int = Field(..., ge=1, le=7, description="Target level")
    resources: List[LearningResourceInfo] = Field(..., description="Resources to rank")
    language: Optional[str] = Field("en", description="Response language (en/vi)")


class RankResourcesResponse(BaseModel):
    """Response from resource ranking."""
    success: bool
    ranked_resources: List[Dict[str, Any]]
    top_recommendations: List[str]
    coverage_assessment: str
    gaps_in_resources: List[str]


@router.post("/generate-learning-path", response_model=GenerateLearningPathResponse)
async def generate_learning_path_endpoint(request: GenerateLearningPathRequest):
    """
    Generate an AI-powered learning path.

    This endpoint:
    1. Takes skill development context (current/target levels)
    2. Optionally considers available resources
    3. Uses Azure OpenAI to generate a comprehensive learning path
    4. Returns structured learning items, milestones, and rationale
    """
    try:
        logger.info(f"Generating learning path for {request.skill_name}: {request.current_level} -> {request.target_level}")

        # Convert resources to dict format
        resources_dict = None
        if request.available_resources:
            resources_dict = [r.dict() for r in request.available_resources]

        result = await generate_learning_path(
            employee_name=request.employee_name,
            skill_name=request.skill_name,
            skill_code=request.skill_code,
            current_level=request.current_level,
            target_level=request.target_level,
            skill_description=request.skill_description,
            available_resources=resources_dict,
            time_constraint_months=request.time_constraint_months,
            language=request.language or "en"
        )

        logger.info(f"Learning path generated: {len(result['learning_items'])} items")
        return GenerateLearningPathResponse(**result)

    except ValueError as e:
        logger.error(f"Learning path generation failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Generation failed: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during learning path generation: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Unexpected error: {str(e)}"
        )


@router.post("/rank-resources", response_model=RankResourcesResponse)
async def rank_resources_endpoint(request: RankResourcesRequest):
    """
    Rank learning resources by relevance for a skill gap.

    This endpoint:
    1. Takes a list of learning resources
    2. Uses Azure OpenAI to evaluate relevance for the skill gap
    3. Returns ranked resources with relevance scores and recommendations
    """
    try:
        logger.info(f"Ranking {len(request.resources)} resources for {request.skill_name}")

        resources_dict = [r.dict() for r in request.resources]

        result = await rank_learning_resources(
            skill_name=request.skill_name,
            skill_code=request.skill_code,
            current_level=request.current_level,
            target_level=request.target_level,
            resources=resources_dict,
            language=request.language or "en"
        )

        logger.info(f"Resource ranking complete: {len(result['ranked_resources'])} ranked")
        return RankResourcesResponse(**result)

    except ValueError as e:
        logger.error(f"Resource ranking failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Ranking failed: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during resource ranking: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Unexpected error: {str(e)}"
        )
