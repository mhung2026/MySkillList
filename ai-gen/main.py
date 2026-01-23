import logging
import sys
from fastapi import FastAPI, HTTPException, Body, status
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import List, Dict, Any, Optional
from datetime import datetime

from src.validators.input_validator import validate_input_skill
from src.validators.output_validator import validate_output_questions
from src.generators.question_generator import generate_questions as generate_questions_from_llm
from config.settings import DEBUG, GEMINI_API_KEY
# from src.custom import getKeywordsTable, getSkillData, getAllSkillsList

# Import V2 routes
from src.api import routes_v2

# Configure logging
logging.basicConfig(
    level=logging.DEBUG if DEBUG else logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    stream=sys.stdout
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="AI Question Generator API",
    description="Generate assessment questions automatically using Google Gemini AI from skill definitions",
    version="0.1.0",
    docs_url="/api/docs",
    redoc_url="/api/redoc",
    openapi_url="/api/openapi.json"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Mount V2 router
app.include_router(routes_v2.router)

# Request/Response models
class SkillLevel(BaseModel):
    level: int = Field(..., ge=1, le=10)
    description: str
    autonomy: Optional[str] = None
    influence: Optional[str] = None
    complexity: Optional[str] = None
    business_skills: Optional[str] = None
    knowledge: Optional[str] = None
    behavioral_indicators: Optional[List[str]] = None
    evidence_examples: Optional[List[str]] = None

class SkillInput(BaseModel):
    skill_name: str
    skill_id: Optional[str] = None
    levels: List[SkillLevel]

class GenerateRequest(BaseModel):
    skill_data: SkillInput = Field(..., description="Skill definition with levels")
    num_questions: int = Field(20, ge=1, le=100, description="Number of questions to generate")
    language: str = Field("vi", description="Language for questions (e.g., 'en', 'vi')")
    min_per_level: int = Field(3, ge=1, description="Minimum questions per proficiency level")

class GenerateResponse(BaseModel):
    success: bool
    timestamp: datetime
    num_questions: int
    questions: List[Dict[str, Any]]

class HealthResponse(BaseModel):
    status: str
    version: str
    api_ready: bool

@app.on_event("startup")
async def startup_event():
    """Startup event handler."""
    logger.info("=" * 50)
    logger.info("AI Question Generator API Starting Up")
    logger.info("=" * 50)
    logger.info(f"DEBUG mode: {DEBUG}")
    logger.info(f"Gemini API configured: {GEMINI_API_KEY is not None}")

@app.on_event("shutdown")
async def shutdown_event():
    """Shutdown event handler."""
    logger.info("=" * 50)
    logger.info("AI Question Generator API Shutting Down")
    logger.info("=" * 50)

@app.get("/", tags=["Info"])
async def root():
    """API information."""
    return {
        "title": "AI Question Generator API",
        "version": "0.1.0",
        "description": "Generate assessment questions using Google Gemini AI",
        "endpoints": {
            "health": "/health",
            "generate": "/generate-questions",
            "docs": "/api/docs",
            "redoc": "/api/redoc"
        }
    }

@app.get("/health", response_model=HealthResponse, tags=["Health"])
async def health_check():
    """Check API health status."""
    try:
        api_ready = GEMINI_API_KEY is not None and len(GEMINI_API_KEY) > 0
    except:
        api_ready = False
    
    return HealthResponse(
        status="healthy",
        version="0.1.0",
        api_ready=api_ready
    )

@app.post(
    "/generate-questions",
    response_model=GenerateResponse,
    status_code=status.HTTP_200_OK,
    tags=["Question Generation"],
    summary="Generate Assessment Questions",
    description="Generate assessment questions for a skill using Google Gemini AI"
)
async def generate_questions_endpoint(request: GenerateRequest = Body(...)):
    """
    Generate assessment questions from a skill definition.
    
    **Request Body:**
    - `skill_data`: Skill definition with proficiency levels
    - `num_questions`: Number of questions to generate (1-100, default 20)
    - `language`: Language for questions (default 'vi' for Vietnamese)
    - `min_per_level`: Minimum questions per proficiency level
    
    **Returns:**
    - `success`: Operation success status
    - `timestamp`: Generation timestamp
    - `num_questions`: Number of questions generated
    - `questions`: Array of generated questions
    """
    logger.info(f"Generating {request.num_questions} questions for skill: {request.skill_data.skill_name}")
    
    # 1. Validate input
    try:
        skill_dict = request.skill_data.dict()
        validate_input_skill(skill_dict)
        logger.info(f"Input validation passed for skill: {request.skill_data.skill_name}")
    except ValueError as e:
        logger.error(f"Input validation failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=f"Invalid skill data: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during input validation: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal validation error"
        )

    # 2. Generate questions using Gemini API
    try:
        logger.info(f"Starting question generation with Gemini API...")
        questions = generate_questions_from_llm(
            skill_json=skill_dict,
            num_questions=request.num_questions,
            language=request.language,
            min_per_level=request.min_per_level
        )
        logger.info(f"Generated {len(questions)} questions successfully")
    except ValueError as e:
        logger.error(f"LLM generation failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to generate questions: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during generation: {str(e)}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Unexpected error during question generation"
        )

    # 3. Validate output
    try:
        validate_output_questions(questions)
        logger.info(f"Output validation passed for {len(questions)} questions")
    except ValueError as e:
        logger.error(f"Output validation failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Generated questions do not match schema: {str(e)}"
        )
    except Exception as e:
        logger.error(f"Unexpected error during output validation: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal validation error"
        )

    return GenerateResponse(
        success=True,
        timestamp=datetime.now(),
        num_questions=len(questions),
        questions=questions
    )

class DocTemplateRequest(BaseModel):
    doc_template: str

class SkillRequest(BaseModel):
    skill_id: Optional[str] = None

@app.get("/test", tags=["Test"])
async def test_endpoint():
    """Simple test endpoint."""
    return {"message": "Test endpoint working"}

@app.post("/get-keywords-table", tags=["Database"])
async def get_keywords_table_endpoint(request: DocTemplateRequest):
    """Get keywords table for a document template."""
    try:
        result = getKeywordsTable(request.doc_template)
        if result is None:
            raise HTTPException(status_code=404, detail=f"Document template '{request.doc_template}' not found")
        return {"doc_template": request.doc_template, "keywords_table": result[0]}
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Database error in get_keywords_table_endpoint: {e}")
        raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")

@app.get("/skills", tags=["Skills"])
async def get_skills_list_endpoint():
    """Get list of all available skills."""
    try:
        skills = getAllSkillsList()
        return {"skills": skills, "total": len(skills)}
    except Exception as e:
        logger.error(f"Database error in get_skills_list_endpoint: {e}")
        raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")

@app.post("/get-skill-data", tags=["Skills"])
async def get_skill_data_endpoint(request: SkillRequest = None):
    """Get skill data formatted for question generation."""
    try:
        skill_data = getSkillData(request.skill_id if request else None)
        if skill_data is None:
            raise HTTPException(status_code=404, detail="No skill data found")
        return skill_data
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Database error in get_skill_data_endpoint: {e}")
        raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")

@app.exception_handler(HTTPException)
# async def get_keywords_table_endpoint(request: DocTemplateRequest):
#     """Get keywords table for a document template."""
#     try:
#         result = getKeywordsTable(request.doc_template)
#         if result is None:
#             raise HTTPException(status_code=404, detail=f"Document template '{request.doc_template}' not found")
#         return {"doc_template": request.doc_template, "keywords_table": result[0]}
#     except HTTPException:
#         raise
#     except Exception as e:
#         logger.error(f"Database error in get_keywords_table_endpoint: {e}")
#         raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")

@app.exception_handler(HTTPException)
async def http_exception_handler(request, exc):
    """Custom HTTP exception handler with logging."""
    logger.error(f"HTTP error {exc.status_code}: {exc.detail}")
    return JSONResponse(
        status_code=exc.status_code,
        content={"error": exc.detail, "status_code": exc.status_code}
    )

if __name__ == "__main__":
    import uvicorn
    logger.info("Starting AI Question Generator API...")
    uvicorn.run(
        app,
        host="127.0.0.1",
        port=8002,
        log_level="debug" if DEBUG else "info"
    )