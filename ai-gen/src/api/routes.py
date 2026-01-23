from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import Dict, List
from ..generators.question_generator import generate_questions
from ..custom import getKeywordsTable

router = APIRouter()

class SkillInput(BaseModel):
    skill: Dict

class DocTemplateRequest(BaseModel):
    doc_template: str

@router.post("/generate-questions", response_model=List[Dict])
async def generate_questions_endpoint(skill_input: SkillInput):
    try:
        questions = generate_questions(skill_input.skill, num_questions=10, language="en", min_per_level=2)
        return questions
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))

@router.post("/get-keywords-table")
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
        raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")