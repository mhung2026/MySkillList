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
