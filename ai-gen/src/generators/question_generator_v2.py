"""
Question Generator V2
Generates questions using Azure OpenAI with output_question_schema_v2
"""

import json
import logging
from typing import Dict, List, Any, Optional
from datetime import datetime
from openai import AsyncOpenAI

from config.settings import OPENAI_API_KEY, OPENAI_BASE_URL, LLM_MODEL

logger = logging.getLogger(__name__)

# Azure OpenAI client (lazy-loaded)
_client = None

def get_client():
    """Get or create Azure OpenAI client."""
    global _client
    if _client is None:
        _client = AsyncOpenAI(
            api_key=OPENAI_API_KEY,
            base_url=OPENAI_BASE_URL
        )
    return _client

def build_prompt_v2(
    normalized_request: Dict[str, Any],
    skill_data: Optional[Dict[str, Any]] = None
) -> str:
    """
    Build prompt for V2 schema with all 9 question types.

    Args:
        normalized_request: Validated and normalized request
        skill_data: Optional skill data from database

    Returns:
        Prompt string for AI
    """
    question_types = normalized_request["question_type"]
    language = normalized_request["language"]  # "en" or "vi"
    num_questions = normalized_request["number_of_questions"]
    difficulty = normalized_request.get("difficulty", "medium")
    context = normalized_request.get("additional_context", "")

    # Language mapping
    lang_name = "English" if language == "en" else "Vietnamese"

    # Build skill context
    skill_context = ""
    skill_id_for_prompt = None

    if skill_data:
        skill_name = skill_data.get("skill_name", "")
        skill_id_for_prompt = skill_data.get("skill_id")
        levels = skill_data.get("levels", [])

        skill_context = f"""
SKILL: {skill_name}
SKILL_ID: {skill_id_for_prompt}

PROFICIENCY LEVELS:
"""
        for level_data in levels:
            level_num = level_data["level"]
            desc = level_data.get("description", "")
            autonomy = level_data.get("autonomy", "")
            influence = level_data.get("influence", "")
            complexity = level_data.get("complexity", "")

            skill_context += f"""
Level {level_num}:
- Description: {desc}
- Autonomy: {autonomy}
- Influence: {influence}
- Complexity: {complexity}
"""
    else:
        skill_context = "SKILL: General technical skill assessment\n"

    # Build additional context
    additional_context_text = ""
    if context:
        additional_context_text = f"""
ADDITIONAL CONTEXT:
{context}
"""

    # Build question type instructions
    type_instructions = """
QUESTION TYPES TO GENERATE:
"""
    for qtype in question_types:
        type_instructions += f"- {qtype}\n"

        # Add specific instructions per type
        if qtype == "MultipleChoice":
            type_instructions += "  * Single correct answer, 2-4 options\n"
        elif qtype == "MultipleAnswer":
            type_instructions += "  * Multiple correct answers possible, 2-4 options\n"
        elif qtype == "TrueFalse":
            type_instructions += "  * Exactly 2 options: True and False\n"
        elif qtype == "ShortAnswer":
            type_instructions += "  * Provide grading_rubric with keywords\n"
        elif qtype == "LongAnswer":
            type_instructions += "  * Provide grading_rubric with criteria\n"
        elif qtype == "CodingChallenge":
            type_instructions += "  * Include code_snippet and grading_rubric with test_cases\n"
        elif qtype == "Scenario":
            type_instructions += "  * Complex scenario with grading_rubric\n"
        elif qtype == "SituationalJudgment":
            type_instructions += "  * 4 options with effectiveness_level (MostEffective/Effective/Ineffective/CounterProductive)\n"
        elif qtype == "Rating":
            type_instructions += "  * 5 rating scale options, all marked as correct\n"

    prompt = f"""You are an expert assessment question generator.

{skill_context}

{additional_context_text}

TASK:
Generate exactly {num_questions} high-quality assessment questions in {lang_name}.

{type_instructions}

DIFFICULTY LEVEL: {difficulty}

OUTPUT REQUIREMENTS:
Return ONLY valid JSON matching this exact schema (no markdown, no explanations):

{{
  "questions": [
    {{
      "skill_id": "{skill_id_for_prompt if skill_id_for_prompt else 'null'}",
      "type": "MultipleChoice|MultipleAnswer|TrueFalse|ShortAnswer|LongAnswer|CodingChallenge|Scenario|SituationalJudgment|Rating",
      "content": "Clear, professional question text in {lang_name}",
      "code_snippet": "Optional code for context",
      "media_url": null,
      "target_level": 1-7,
      "difficulty": "Easy|Medium|Hard",
      "points": 5-30,
      "time_limit_seconds": 60-900,
      "tags": ["relevant", "tags"],
      "options": [
        {{
          "content": "Option text",
          "is_correct": true|false,
          "display_order": 1,
          "explanation": "Why correct/incorrect",
          "effectiveness_level": "MostEffective|Effective|Ineffective|CounterProductive"  // Only for SJT
        }}
      ],
      "grading_rubric": "{{\\"criteria\\":[...]}}" or null,  // JSON string for text/coding questions
      "explanation": "Answer explanation",
      "hints": ["helpful hint 1", "helpful hint 2"]
    }}
  ]
}}

IMPORTANT RULES:
1. Use the exact skill_id provided above for ALL questions (do not generate random UUIDs)
2. For MultipleChoice: Exactly 1 option with is_correct=true
3. For MultipleAnswer: 2+ options with is_correct=true
4. For TrueFalse: Exactly 2 options (True/False)
5. For ShortAnswer/LongAnswer: Include grading_rubric as JSON string
6. For CodingChallenge: Include code_snippet and grading_rubric with test_cases
7. For SituationalJudgment: 4 options with effectiveness_level
8. For Rating: 3-5 options, all with is_correct=true
9. Use clear, professional language
10. Ensure questions are at appropriate difficulty level
11. Return ONLY the JSON, no markdown blocks or explanations

Generate questions that are:
- Practical and relevant to real-world scenarios
- Clear and unambiguous
- Appropriate for the skill level
- Diverse in coverage across proficiency levels
"""

    return prompt


async def generate_questions_v2(
    normalized_request: Dict[str, Any],
    skill_data: Optional[Dict[str, Any]] = None
) -> Dict[str, Any]:
    """
    Generate questions using Azure OpenAI with V2 schema.

    Args:
        normalized_request: Validated request from request_validator
        skill_data: Optional skill data from database

    Returns:
        Dict with 'questions' and 'metadata' matching output_question_schema_v2
    """
    logger.info("Starting AI question generation with Azure OpenAI")
    logger.debug(f"Request: {normalized_request}")

    try:
        # 1. Build prompt
        prompt = build_prompt_v2(normalized_request, skill_data)
        logger.debug(f"Prompt built: {len(prompt)} characters")

        # 2. Call Azure OpenAI API
        logger.info(f"Calling Azure OpenAI API with model: {LLM_MODEL}")

        client = get_client()
        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert assessment question generator. You always return valid JSON without markdown formatting."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.7,
            max_tokens=8192,
            response_format={"type": "json_object"}
        )

        logger.info("Received response from Azure OpenAI")

        # 3. Parse response
        response_text = response.choices[0].message.content.strip()
        logger.debug(f"Response length: {len(response_text)} characters")

        # Remove markdown code blocks if present
        if response_text.startswith("```"):
            logger.debug("Removing markdown code blocks")
            response_text = response_text.split("```")[1]
            if response_text.startswith("json"):
                response_text = response_text[4:]
            response_text = response_text.strip()

        # 4. Parse JSON
        try:
            result = json.loads(response_text)
        except json.JSONDecodeError as e:
            logger.error(f"JSON parse error: {e}")
            logger.error(f"Response text: {response_text[:500]}...")
            raise ValueError(f"Failed to parse AI response as JSON: {str(e)}")

        # 5. Validate structure
        if "questions" not in result:
            logger.warning("Response missing 'questions' key, attempting to wrap")
            if isinstance(result, list):
                result = {"questions": result}
            else:
                raise ValueError("Response does not contain 'questions' array")

        questions = result["questions"]
        logger.info(f"Generated {len(questions)} questions")

        # 5.5. Ensure each question has skill_id from request
        skill_id_from_request = None
        if skill_data and "skill_id" in skill_data:
            skill_id_from_request = skill_data["skill_id"]
        elif normalized_request.get("skills") and len(normalized_request["skills"]) > 0:
            skill_id_from_request = normalized_request["skills"][0].get("skill_id")

        # Inject skill_id into each question if not already present or if null
        if skill_id_from_request:
            for question in questions:
                if not question.get("skill_id") or question.get("skill_id") == "null":
                    question["skill_id"] = skill_id_from_request
                    logger.debug(f"Injected skill_id {skill_id_from_request} into question")

        # 6. Add metadata
        metadata = {
            "total_questions": len(questions),
            "generation_timestamp": datetime.now().isoformat(),
            "ai_model": LLM_MODEL,
            "skill_id": skill_data.get("skill_id") if skill_data else None,
            "skill_name": skill_data.get("skill_name") if skill_data else None,
            "language": normalized_request["language"]
        }

        # 7. Return V2 format
        output = {
            "questions": questions,
            "metadata": metadata
        }

        logger.info("Successfully generated questions")
        return output

    except Exception as e:
        logger.error(f"Error generating questions: {e}", exc_info=True)
        raise ValueError(f"Failed to generate questions: {str(e)}")


async def test_generator():
    """Test the generator with sample data."""
    test_request = {
        "question_type": ["MultipleChoice", "ShortAnswer"],
        "language": "en",
        "number_of_questions": 3,
        "difficulty": "medium",
        "additional_context": "Focus on practical scenarios"
    }

    test_skill = {
        "skill_id": "test-uuid",
        "skill_name": "Python Programming",
        "levels": [
            {
                "level": 3,
                "description": "Can write Python code independently",
                "autonomy": "Works with general direction",
                "influence": "Influences team coding practices",
                "complexity": "Moderate complexity tasks"
            }
        ]
    }

    result = await generate_questions_v2(test_request, test_skill)
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    import asyncio
    logging.basicConfig(level=logging.DEBUG)
    asyncio.run(test_generator())
