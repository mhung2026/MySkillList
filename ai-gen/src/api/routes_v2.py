"""
AI Question Generation API V2
Endpoints for generating questions and grading answers with new schema
"""

from fastapi import APIRouter, HTTPException, status
from pydantic import BaseModel, Field
from typing import List, Optional, Dict, Any
from datetime import datetime
import json
import logging

from ..validators.request_validator import validate_and_normalize, RequestValidator
from db_skill_reader import (
    getDistinctSkillsWithLevels,
    getSkillLevelsBySkillId,
    getSkillLevelCount,
    getCourseraCoursesBySkillId,
    getEmployeeById
)
from ..generators.question_generator_v2 import generate_questions_v2 as ai_generate_questions
from ..generators.answer_grader import grade_answer as ai_grade_answer
from ..generators.skill_gap_analyzer import analyze_skill_gap, analyze_multiple_gaps
from ..generators.learning_path_recommender import generate_learning_path, generate_multiple_learning_paths, rank_learning_resources
from ..generators.assessment_evaluator import evaluate_assessment, evaluate_multiple_skills

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
    """Request to grade a submitted answer."""
    question_id: Optional[str] = Field(None, description="Question ID (optional)")
    question_content: str = Field(..., description="The question text")
    submitted_answer: Optional[str] = Field("", description="Candidate's submitted answer (null/empty treated as no answer)")
    max_points: int = Field(..., ge=1, description="Maximum points for this question")
    grading_rubric: Optional[Any] = Field(None, description="Grading criteria (JSON string or dict)")
    expected_answer: Optional[str] = Field(None, description="Expected/model answer")
    question_type: Optional[str] = Field(None, description="Question type (ShortAnswer, LongAnswer, Scenario, etc.)")
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
    Grade a submitted answer using AI.

    This endpoint:
    1. Takes the question, submitted answer, and grading criteria
    2. Uses Azure OpenAI to evaluate the answer
    3. Returns points, feedback, strengths, and improvement areas
    """
    try:
        # Normalize submitted_answer: null/None → empty string
        submitted_answer = request.submitted_answer or ""
        if isinstance(submitted_answer, (int, float)):
            submitted_answer = str(submitted_answer)

        # Normalize grading_rubric: dict → JSON string
        grading_rubric = request.grading_rubric
        if isinstance(grading_rubric, dict):
            grading_rubric = json.dumps(grading_rubric)
        elif grading_rubric is not None and not isinstance(grading_rubric, str):
            grading_rubric = str(grading_rubric)

        logger.info(f"Grading answer for question (max_points={request.max_points}, "
                     f"answer_len={len(submitted_answer)}, type={request.question_type})")
        logger.debug(f"Question: {request.question_content[:100]}...")

        # Call AI grader
        result = await ai_grade_answer(
            question_content=request.question_content,
            submitted_answer=submitted_answer,
            max_points=request.max_points,
            grading_rubric=grading_rubric,
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
    employee_id: str = Field(..., description="Employee ID (UUID)")
    job_role: Optional[str] = Field(None, description="Job role name (auto-resolved from employee if omitted)")
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
    employee_id: str = Field(..., description="Employee ID (UUID)")
    job_role: Optional[str] = Field(None, description="Job role name (auto-resolved from employee if omitted)")
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
        # Look up employee from DB
        employee = getEmployeeById(request.employee_id)
        if not employee:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Employee not found: {request.employee_id}"
            )
        employee_name = employee["FullName"]
        job_role = request.job_role or employee.get("JobRole") or "Unknown"

        logger.info(f"Analyzing gap for {request.skill_name}: {request.current_level} -> {request.required_level} (employee: {employee_name})")

        result = await analyze_skill_gap(
            employee_name=employee_name,
            job_role=job_role,
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

    except HTTPException:
        raise
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
        # Look up employee from DB
        employee = getEmployeeById(request.employee_id)
        if not employee:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Employee not found: {request.employee_id}"
            )
        employee_name = employee["FullName"]
        job_role = request.job_role or employee.get("JobRole") or "Unknown"

        logger.info(f"Analyzing {len(request.gaps)} gaps for {employee_name}")

        result = await analyze_multiple_gaps(
            employee_name=employee_name,
            job_role=job_role,
            gaps=request.gaps,
            language=request.language or "en"
        )

        logger.info(f"Multiple gaps analysis complete: {len(result['gap_analyses'])} analyzed")
        return MultipleGapsResponse(**result)

    except HTTPException:
        raise
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
    employee_id: str = Field(..., description="Employee ID (UUID)")
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


class LearningPathSkillInfo(BaseModel):
    """Skill info for multi-skill learning path generation."""
    skill_id: Optional[str] = Field(None, description="Skill UUID (used to fetch Coursera courses)")
    skill_name: str = Field(..., description="Skill name")
    skill_code: str = Field(..., description="SFIA skill code")
    current_level: int = Field(..., ge=0, le=7, description="Current proficiency level (0-7)")
    target_level: int = Field(..., ge=1, le=7, description="Target proficiency level (1-7)")
    skill_description: Optional[str] = Field(None, description="Skill description")


class GenerateMultipleLearningPathsRequest(BaseModel):
    """Request to generate learning paths for multiple skills."""
    employee_id: str = Field(..., description="Employee ID (UUID)")
    skills: List[LearningPathSkillInfo] = Field(..., min_items=1, description="Skills to generate paths for")
    time_constraint_months: Optional[int] = Field(None, ge=1, le=24, description="Time constraint in months")
    language: Optional[str] = Field("en", description="Response language (en/vi)")


class GenerateMultipleLearningPathsResponse(BaseModel):
    """Response from multi-skill learning path generation."""
    success: bool
    learning_paths: List[Dict[str, Any]]
    overall_summary: str
    recommended_learning_order: List[str]


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


def _parse_duration_to_hours(duration_str: str | None) -> int | None:
    """Parse Coursera duration strings to approximate hours.

    Handles formats like:
    - '6 hours to complete'
    - '1 week at 10 hours a week'
    - '3 months at 5 hours a week'
    - '2 weeks'
    """
    if not duration_str:
        return None
    d = duration_str.lower().strip()
    import re

    # Try "X week/month at Y hours a week" pattern first
    match_detailed = re.match(r'(\d+)\s*(week|month)s?\s+at\s+(\d+)\s*hours?\s+a\s+week', d)
    if match_detailed:
        num = int(match_detailed.group(1))
        unit = match_detailed.group(2)
        hours_per_week = int(match_detailed.group(3))
        weeks = num if unit == "week" else num * 4
        return weeks * hours_per_week

    # Try simple "X hours to complete" or "X hours"
    match_hours = re.match(r'(\d+)\s*hours?', d)
    if match_hours:
        return int(match_hours.group(1))

    # Try simple "X weeks/months/days"
    match_simple = re.match(r'(\d+)\s*(week|month|day)s?', d)
    if not match_simple:
        return None
    num = int(match_simple.group(1))
    unit = match_simple.group(2)
    if unit == "day":
        return num * 2
    elif unit == "week":
        return num * 5  # ~5 hours/week default
    elif unit == "month":
        return num * 20  # ~20 hours/month
    return None


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
        # Look up employee from DB
        employee = getEmployeeById(request.employee_id)
        if not employee:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Employee not found: {request.employee_id}"
            )
        employee_name = employee["FullName"]

        logger.info(f"Generating learning path for {employee_name} - {request.skill_name}: {request.current_level} -> {request.target_level}")

        # Convert caller-provided resources to dict format
        resources_dict = []
        if request.available_resources:
            resources_dict = [r.dict() for r in request.available_resources]

        # Fetch real Coursera courses from DB by skill_id, filtered by level
        coursera_items = []
        if request.skill_id:
            try:
                raw_courses = getCourseraCoursesBySkillId(request.skill_id)
                cur = request.current_level
                tgt = request.target_level
                filtered = []
                for c in raw_courses:
                    course_level = (c.get("Level") or "").strip().lower()
                    if course_level in ("", "n/a"):
                        # No level info → always include
                        filtered.append(c)
                    elif course_level == "beginner level":
                        # Beginner → include if learning range covers 1-2
                        if cur <= 2:
                            filtered.append(c)
                    elif course_level == "intermediate level":
                        # Intermediate → include if learning range covers 3-4
                        if cur <= 4 and tgt >= 3:
                            filtered.append(c)
                    else:
                        # Advanced / Mixed / other → include if target > 4
                        if tgt > 4:
                            filtered.append(c)

                for i, c in enumerate(filtered, 1):
                    coursera_items.append({
                        "order": i,
                        "title": c.get("Title") or "Untitled Course",
                        "description": c.get("Description") or "",
                        "item_type": "Course",
                        "source": "Coursera",
                        "estimated_hours": _parse_duration_to_hours(c.get("Duration")) or 0,
                        "target_level_after": 0,
                        "success_criteria": "",
                        "resource_id": str(c["Id"]),
                        "url": c.get("Url"),
                        "organization": c.get("Organization"),
                        "difficulty": c.get("Level"),
                        "rating": float(c["Rating"]) if c.get("Rating") else None,
                        "reviews_count": c.get("ReviewsCount"),
                        "certificate_available": c.get("CertificateAvailable"),
                    })
                logger.info(f"Fetched {len(raw_courses)} courses, filtered to {len(coursera_items)} for levels {cur}->{tgt}")
            except Exception as e:
                logger.warning(f"Failed to fetch Coursera courses: {e}")

        # Generate AI learning path metadata (title, description, milestones, rationale)
        result = await generate_learning_path(
            employee_name=employee_name,
            skill_name=request.skill_name,
            skill_code=request.skill_code,
            current_level=request.current_level,
            target_level=request.target_level,
            skill_description=request.skill_description,
            available_resources=resources_dict if resources_dict else None,
            time_constraint_months=request.time_constraint_months,
            language=request.language or "en"
        )

        # learning_items = ONLY real Coursera courses from DB
        result["learning_items"] = coursera_items

        logger.info(f"Learning path generated with {len(coursera_items)} Coursera courses from DB")
        return GenerateLearningPathResponse(**result)

    except HTTPException:
        raise
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


@router.post("/generate-learning-paths", response_model=GenerateMultipleLearningPathsResponse)
async def generate_multiple_learning_paths_endpoint(request: GenerateMultipleLearningPathsRequest):
    """
    Generate AI-powered learning paths for multiple skills in a single AI call.

    This endpoint:
    1. Resolves employee from DB
    2. Fetches Coursera courses per skill from DB
    3. Calls AI once for all skills (not N loops)
    4. Returns learning paths with Coursera courses merged, plus cross-skill analysis
    """
    try:
        # Look up employee from DB
        employee = getEmployeeById(request.employee_id)
        if not employee:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Employee not found: {request.employee_id}"
            )
        employee_name = employee["FullName"]

        logger.info(f"Generating learning paths for {len(request.skills)} skills for {employee_name}")

        # Fetch Coursera courses per skill (DB queries, fast)
        coursera_by_skill = {}  # skill_code -> coursera_items[]
        for skill in request.skills:
            if skill.skill_id:
                try:
                    raw_courses = getCourseraCoursesBySkillId(skill.skill_id)
                    cur = skill.current_level
                    tgt = skill.target_level
                    filtered = []
                    for c in raw_courses:
                        course_level = (c.get("Level") or "").strip().lower()
                        if course_level in ("", "n/a"):
                            filtered.append(c)
                        elif course_level == "beginner level":
                            if cur <= 2:
                                filtered.append(c)
                        elif course_level == "intermediate level":
                            if cur <= 4 and tgt >= 3:
                                filtered.append(c)
                        else:
                            if tgt > 4:
                                filtered.append(c)

                    items = []
                    for i, c in enumerate(filtered, 1):
                        items.append({
                            "order": i,
                            "title": c.get("Title") or "Untitled Course",
                            "description": c.get("Description") or "",
                            "item_type": "Course",
                            "source": "Coursera",
                            "estimated_hours": _parse_duration_to_hours(c.get("Duration")) or 0,
                            "target_level_after": 0,
                            "success_criteria": "",
                            "resource_id": str(c["Id"]),
                            "url": c.get("Url"),
                            "organization": c.get("Organization"),
                            "difficulty": c.get("Level"),
                            "rating": float(c["Rating"]) if c.get("Rating") else None,
                            "reviews_count": c.get("ReviewsCount"),
                            "certificate_available": c.get("CertificateAvailable"),
                        })
                    coursera_by_skill[skill.skill_code] = items
                    logger.info(f"Fetched {len(raw_courses)} courses for {skill.skill_code}, filtered to {len(items)}")
                except Exception as e:
                    logger.warning(f"Failed to fetch Coursera courses for {skill.skill_code}: {e}")
                    coursera_by_skill[skill.skill_code] = []
            else:
                coursera_by_skill[skill.skill_code] = []

        # Single AI call for all skills
        skills_for_ai = [
            {
                "skill_name": s.skill_name,
                "skill_code": s.skill_code,
                "current_level": s.current_level,
                "target_level": s.target_level,
                "skill_description": s.skill_description,
            }
            for s in request.skills
        ]

        result = await generate_multiple_learning_paths(
            employee_name=employee_name,
            skills=skills_for_ai,
            time_constraint_months=request.time_constraint_months,
            language=request.language or "en"
        )

        # Merge Coursera courses into each path
        for path in result.get("learning_paths", []):
            skill_code = path.get("skill_code", "")
            coursera_items = coursera_by_skill.get(skill_code, [])
            path["learning_items"] = coursera_items
            # Add skill_id and skill_name from request
            for s in request.skills:
                if s.skill_code == skill_code:
                    path["skill_id"] = s.skill_id
                    path["skill_name"] = s.skill_name
                    break

        logger.info(f"Multi-skill learning paths generated: {len(result.get('learning_paths', []))} paths")
        return GenerateMultipleLearningPathsResponse(**result)

    except HTTPException:
        raise
    except ValueError as e:
        logger.error(f"Multi-learning-path generation failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Generation failed: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during multi-learning-path generation: {e}", exc_info=True)
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


# ============================================================================
# ASSESSMENT EVALUATION ENDPOINTS
# ============================================================================

class AssessmentResponseItem(BaseModel):
    """Individual response item in an assessment."""
    question_id: str = Field(..., description="Question ID")
    question_type: str = Field(..., description="Question type (MultipleChoice, SituationalJudgment, etc.)")
    target_level: int = Field(..., ge=1, le=7, description="Target SFIA level for this question")
    is_correct: Optional[bool] = Field(None, description="Whether the answer is correct (for objective types)")
    score: Optional[float] = Field(None, ge=0, le=100, description="Score awarded (for graded types)")
    max_score: Optional[float] = Field(100, description="Maximum possible score (for graded types)")


class EvaluateAssessmentRequest(BaseModel):
    """Request to evaluate an assessment and determine CurrentLevel."""
    skill_id: str = Field(..., description="Skill ID being assessed")
    skill_name: Optional[str] = Field(None, description="Skill name (optional)")
    responses: List[AssessmentResponseItem] = Field(..., min_items=1, description="List of assessment responses")


class LevelResultItem(BaseModel):
    """Result for a single level."""
    total: int
    correct: int
    percentage: float
    passed: bool


class EvaluationBreakdownItem(BaseModel):
    """Breakdown item for evaluation details."""
    level: int
    status: str
    percentage: Optional[float] = None
    message: str


class EvaluationDetails(BaseModel):
    """Detailed evaluation information."""
    method: str
    threshold: float
    start_level: Optional[int] = None
    end_level: Optional[int] = None
    breakdown: List[EvaluationBreakdownItem]
    message: Optional[str] = None


class EvaluateAssessmentResponse(BaseModel):
    """Response from assessment evaluation."""
    skill_id: str
    skill_name: Optional[str] = None
    current_level: int = Field(..., ge=0, le=7, description="Determined SFIA level (0 if first level not passed)")
    min_defined_level: int = Field(..., ge=1, le=7, description="Skill's lowest defined SFIA level (consecutive check starts here)")
    max_defined_level: int = Field(..., ge=1, le=7, description="Skill's highest defined SFIA level (consecutive check ends here)")
    level_results: Dict[str, LevelResultItem]
    consecutive_levels_passed: int
    highest_level_with_responses: int
    total_questions: int
    overall_score_percentage: float
    evaluation_details: EvaluationDetails


class EvaluateMultipleSkillsRequest(BaseModel):
    """Request to evaluate multiple skill assessments."""
    assessments: List[EvaluateAssessmentRequest] = Field(..., min_items=1, description="List of skill assessments")


class EvaluationSummary(BaseModel):
    """Summary of multiple skill evaluations."""
    total_skills: int
    skills_evaluated: int
    average_level: float


class EvaluateMultipleSkillsResponse(BaseModel):
    """Response from multiple skill evaluations."""
    results: List[EvaluateAssessmentResponse]
    summary: EvaluationSummary


@router.post("/evaluate-assessment", response_model=EvaluateAssessmentResponse)
async def evaluate_assessment_endpoint(request: EvaluateAssessmentRequest):
    """
    Evaluate an assessment and determine CurrentLevel using SFIA bottom-up logic.

    This endpoint:
    1. Groups responses by target_level
    2. Calculates % correct per level (70% threshold)
    3. Determines CurrentLevel = highest CONSECUTIVE level where ALL levels 1→L pass

    Example:
    - If L1=80%, L2=75%, L3=60%, L4=85% → CurrentLevel = 2
      (L3 failed, so consecutive chain breaks at L2)
    - If L1=65%, L2=90%, L3=90% → CurrentLevel = 0
      (L1 failed, so no consecutive levels passed)
    """
    try:
        logger.info(f"Evaluating assessment for skill: {request.skill_id}")
        logger.debug(f"Number of responses: {len(request.responses)}")

        # Query DB for skill's available levels
        available_levels = None
        try:
            levels = getSkillLevelsBySkillId(request.skill_id)
            if levels:
                available_levels = sorted([l[0] for l in levels])
                logger.info(f"Skill {request.skill_id} has defined levels: {available_levels}")
        except Exception as e:
            logger.warning(f"Could not fetch skill levels from DB: {e}")

        # Convert to dict format for evaluator
        assessment_data = {
            "skill_id": request.skill_id,
            "skill_name": request.skill_name,
            "responses": [r.dict() for r in request.responses]
        }

        result = evaluate_assessment(assessment_data, available_levels=available_levels)

        logger.info(f"Evaluation complete: CurrentLevel = {result['current_level']} (min_defined_level={result['min_defined_level']})")
        return EvaluateAssessmentResponse(**result)

    except Exception as e:
        logger.error(f"Assessment evaluation failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Evaluation failed: {str(e)}"
        )


@router.post("/evaluate-assessments", response_model=EvaluateMultipleSkillsResponse)
async def evaluate_multiple_assessments_endpoint(request: EvaluateMultipleSkillsRequest):
    """
    Evaluate multiple skill assessments at once.

    This endpoint:
    1. Evaluates each skill assessment independently
    2. Returns individual results for each skill
    3. Provides summary with average level across all skills
    """
    try:
        logger.info(f"Evaluating {len(request.assessments)} skill assessments")

        # Evaluate each skill individually with its available levels from DB
        results = []
        total_levels = 0
        skills_with_level = 0

        for a in request.assessments:
            # Query DB for each skill's available levels
            available_levels = None
            try:
                levels = getSkillLevelsBySkillId(a.skill_id)
                if levels:
                    available_levels = sorted([l[0] for l in levels])
            except Exception as e:
                logger.warning(f"Could not fetch levels for skill {a.skill_id}: {e}")

            assessment_data = {
                "skill_id": a.skill_id,
                "skill_name": a.skill_name,
                "responses": [r.dict() for r in a.responses]
            }

            result = evaluate_assessment(assessment_data, available_levels=available_levels)
            results.append(result)

            if result["current_level"] > 0:
                total_levels += result["current_level"]
                skills_with_level += 1

        average_level = round(total_levels / skills_with_level, 1) if skills_with_level > 0 else 0

        final_result = {
            "results": results,
            "summary": {
                "total_skills": len(request.assessments),
                "skills_evaluated": skills_with_level,
                "average_level": average_level
            }
        }

        logger.info(f"Multiple evaluations complete: {len(results)} skills, avg level = {average_level}")
        return EvaluateMultipleSkillsResponse(**final_result)

    except Exception as e:
        logger.error(f"Multiple assessment evaluation failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Evaluation failed: {str(e)}"
        )
