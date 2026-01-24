import json
from openai import AzureOpenAI
from typing import Dict, List
from ..validators.input_validator import validate_input_skill
from ..validators.output_validator import validate_output_questions
from config.settings import OPENAI_API_KEY, OPENAI_BASE_URL, LLM_MODEL

# Configure Azure OpenAI client
client = AzureOpenAI(
    api_key=OPENAI_API_KEY,
    api_version="2024-02-15-preview",
    azure_endpoint=OPENAI_BASE_URL.rstrip('/openai/v1/') if OPENAI_BASE_URL else None
)

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

    prompt += f"""
Generate questions with varied types. Return a JSON array where each question MUST have these REQUIRED fields:
- "id": unique string like "q1", "q2", etc (REQUIRED)
- "type": one of "mcq", "true_false", "short_answer", "essay" (REQUIRED)
- "stem": the question text (REQUIRED)
- "language": "{language}" (REQUIRED)
- "difficulty": one of "easy", "medium", "hard" (REQUIRED)
- "topic": "{skill_name}"

For MCQ type, include "choices" array with at least 2 items:
{{"id": "a", "text": "option text", "is_correct": true/false}}

For true_false type, include "answer": true or false

For short_answer type, include "expected_answer": "expected response"

Example MCQ:
{{"id": "q1", "type": "mcq", "stem": "Question?", "language": "{language}", "difficulty": "easy", "topic": "{skill_name}", "choices": [{{"id": "a", "text": "Option A", "is_correct": true}}, {{"id": "b", "text": "Option B", "is_correct": false}}]}}

Example true_false:
{{"id": "q2", "type": "true_false", "stem": "Statement", "language": "{language}", "difficulty": "medium", "topic": "{skill_name}", "answer": true}}

Example short_answer:
{{"id": "q3", "type": "short_answer", "stem": "Question?", "language": "{language}", "difficulty": "hard", "topic": "{skill_name}", "expected_answer": "Expected response"}}

Return ONLY the JSON array, no markdown code blocks or extra text."""
    return prompt

def generate_questions(skill_json: dict, num_questions: int, language: str, min_per_level: int) -> List[Dict]:
    """
    Generate questions using Azure OpenAI API.
    """
    # Validate input
    validate_input_skill(skill_json)

    # Build prompt
    prompt = build_prompt(skill_json, num_questions, language)

    try:
        # Call Azure OpenAI API
        response = client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {"role": "system", "content": "You are an expert at creating assessment questions. Always respond with valid JSON only."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.7,
            max_tokens=4000
        )

        # Extract JSON from response
        response_text = response.choices[0].message.content.strip()
        
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