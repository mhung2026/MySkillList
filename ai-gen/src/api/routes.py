from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import Dict, List
from ..generators.question_generator import generate_questions

router = APIRouter()

class SkillInput(BaseModel):
    skill: Dict

@router.post("/generate-questions", response_model=List[Dict])
async def generate_questions_endpoint(skill_input: SkillInput):
    try:
        questions = generate_questions(skill_input.skill, num_questions=10, language="en", min_per_level=2)
        return questions
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
