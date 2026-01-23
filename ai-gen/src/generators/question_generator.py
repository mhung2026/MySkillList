import json
import google.generativeai as genai
from typing import Dict, List
from ..validators.input_validator import validate_input_skill
from ..validators.output_validator import validate_output_questions
from config.settings import GEMINI_API_KEY, LLM_MODEL

# Configure Gemini API
genai.configure(api_key=GEMINI_API_KEY)

def build_prompt(skill_json: dict, num_questions: int, language: str) -> str:
    """Build a detailed prompt for question generation."""
    skill_name = skill_json.get("skill_name", "Unknown Skill")
    levels = skill_json.get("levels", [])
    
    prompt = f"""Generate exactly {num_questions} assessment questions in {language} for the skill: {skill_name}.

Proficiency Levels:
"""
    for level in levels:
        prompt += f"- Level {level['level']}: {level['description']}\n"
        if level.get('autonomy'):
            prompt += f"  Autonomy: {level['autonomy']}\n"
        if level.get('influence'):
            prompt += f"  Influence: {level['influence']}\n"
        if level.get('complexity'):
            prompt += f"  Complexity: {level['complexity']}\n"

    prompt += """
Generate questions with varied types:
- MCQ (multiple choice with 2+ options, at least 1 correct)
- True/False
- Short Answer
- Essay
- Coding (optional)

Return ONLY valid JSON array of questions matching this schema:
{
  "id": "q1",
  "type": "mcq|true_false|short_answer|essay|coding",
  "stem": "question text",
  "language": "{language}",
  "difficulty": "easy|medium|hard",
  "topic": "{skill_name}",
  "subtopic": "optional subtopic",
  ...
  "choices": [...] (for mcq only, min 2 items with 1 correct),
  "answer": boolean (for true_false only),
  "expected_answer": "string" (for short_answer),
  ...
}

Return ONLY the JSON array, no markdown or extra text."""
    return prompt

def generate_questions(skill_json: dict, num_questions: int, language: str, min_per_level: int) -> List[Dict]:
    """
    Generate questions using Google Gemini API.
    """
    # Validate input
    validate_input_skill(skill_json)
    
    # Build prompt
    prompt = build_prompt(skill_json, num_questions, language)
    
    try:
        # Call Gemini API
        model = genai.GenerativeModel(LLM_MODEL)
        response = model.generate_content(prompt)
        
        # Extract JSON from response
        response_text = response.text.strip()
        
        # Remove markdown code blocks if present
        if response_text.startswith("```"):
            response_text = response_text.split("```")[1]
            if response_text.startswith("json"):
                response_text = response_text[4:]
            response_text = response_text.strip()
        
        # Parse JSON
        questions = json.loads(response_text)
        
        # Ensure it's a list
        if not isinstance(questions, list):
            questions = [questions]
        
        # Validate output
        validate_output_questions(questions)
        
        return questions
    
    except json.JSONDecodeError as e:
        raise ValueError(f"Failed to parse LLM response as JSON: {str(e)}")
    except Exception as e:
        raise ValueError(f"Failed to generate questions: {str(e)}")