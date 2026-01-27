"""
Answer Grader
Grades submitted answers using Azure OpenAI based on rubric and expected answer
"""

import json
import logging
from typing import Dict, Any, Optional
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


def build_grading_prompt(
    question_content: str,
    submitted_answer: str,
    max_points: int,
    grading_rubric: Optional[str] = None,
    expected_answer: Optional[str] = None,
    question_type: Optional[str] = None,
    language: str = "en"
) -> str:
    """
    Build prompt for grading submitted answer.

    Args:
        question_content: The question text
        submitted_answer: Candidate's submitted answer
        max_points: Maximum points for this question
        grading_rubric: JSON string with grading criteria (optional)
        expected_answer: Expected/model answer (optional)
        question_type: Type of question (ShortAnswer, LongAnswer, CodingChallenge, etc.)
        language: Response language (en/vi)

    Returns:
        Prompt string for AI grading
    """
    lang_name = "English" if language == "en" else "Vietnamese"

    # Parse grading rubric if provided
    rubric_text = ""
    if grading_rubric:
        try:
            rubric = json.loads(grading_rubric) if isinstance(grading_rubric, str) else grading_rubric
            if isinstance(rubric, dict):
                if "criteria" in rubric:
                    rubric_text = "GRADING CRITERIA:\n"
                    for i, criterion in enumerate(rubric["criteria"], 1):
                        desc = criterion.get("description", "")
                        points = criterion.get("points", 0)
                        rubric_text += f"{i}. {desc} ({points} points)\n"
                elif "keywords" in rubric:
                    rubric_text = f"EXPECTED KEYWORDS: {', '.join(rubric['keywords'])}\n"
                elif "test_cases" in rubric:
                    rubric_text = "TEST CASES:\n"
                    for tc in rubric["test_cases"]:
                        rubric_text += f"- Input: {tc.get('input', 'N/A')}, Expected: {tc.get('expected', 'N/A')}\n"
        except (json.JSONDecodeError, TypeError):
            rubric_text = f"GRADING RUBRIC: {grading_rubric}\n"

    # Build expected answer section
    expected_text = ""
    if expected_answer:
        expected_text = f"""
EXPECTED/MODEL ANSWER:
{expected_answer}
"""

    prompt = f"""You are an expert assessment grader. Grade the candidate's answer objectively and fairly.

QUESTION:
{question_content}

SUBMITTED ANSWER:
{submitted_answer}

{expected_text}

{rubric_text}

MAXIMUM POINTS: {max_points}

GRADING INSTRUCTIONS:
1. Evaluate the candidate's answer against the criteria/expected answer
2. Be fair but rigorous - partial credit is allowed
3. Consider both correctness and completeness
4. For coding questions, evaluate logic even if syntax has minor issues
5. Provide constructive feedback in {lang_name}

OUTPUT REQUIREMENTS:
Return ONLY valid JSON matching this exact schema (no markdown, no explanations):

{{
  "points_awarded": <number between 0 and {max_points}>,
  "max_points": {max_points},
  "percentage": <calculated percentage>,
  "feedback": "Brief overall feedback in {lang_name}",
  "strength_points": ["What the candidate did well", "Another strength"],
  "improvement_areas": ["What could be improved", "Another area"],
  "detailed_analysis": "Detailed explanation of the grading decision in {lang_name}"
}}

IMPORTANT:
- points_awarded must be between 0 and {max_points}
- percentage = (points_awarded / max_points) * 100
- Be specific in feedback - reference parts of the candidate's answer
- strength_points and improvement_areas should have 1-3 items each
- Return ONLY the JSON, no markdown blocks
"""

    return prompt


async def grade_answer(
    question_content: str,
    submitted_answer: str,
    max_points: int,
    grading_rubric: Optional[str] = None,
    expected_answer: Optional[str] = None,
    question_type: Optional[str] = None,
    language: str = "en"
) -> Dict[str, Any]:
    """
    Grade a submitted answer using Azure OpenAI.

    Args:
        question_content: The question text
        submitted_answer: Candidate's submitted answer
        max_points: Maximum points for this question
        grading_rubric: JSON string with grading criteria
        expected_answer: Expected/model answer
        question_type: Type of question
        language: Response language

    Returns:
        Dict with grading result
    """
    logger.info(f"Grading answer for question (max_points={max_points})")
    logger.debug(f"Submitted answer length: {len(submitted_answer)} chars")

    # Handle empty answers
    if not submitted_answer or submitted_answer.strip() == "":
        logger.warning("Empty submitted answer received")
        return {
            "success": True,
            "points_awarded": 0,
            "max_points": max_points,
            "percentage": 0.0,
            "feedback": "No answer was provided." if language == "en" else "Không có câu trả lời được cung cấp.",
            "strength_points": [],
            "improvement_areas": ["Submit an answer to receive feedback" if language == "en" else "Hãy gửi câu trả lời để nhận phản hồi"],
            "detailed_analysis": None
        }

    try:
        # Build prompt
        prompt = build_grading_prompt(
            question_content=question_content,
            submitted_answer=submitted_answer,
            max_points=max_points,
            grading_rubric=grading_rubric,
            expected_answer=expected_answer,
            question_type=question_type,
            language=language
        )
        logger.debug(f"Grading prompt built: {len(prompt)} characters")

        # Call Azure OpenAI
        logger.info(f"Calling Azure OpenAI for grading with model: {LLM_MODEL}")
        client = get_client()

        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert assessment grader. You evaluate submitted answers objectively and provide constructive feedback. Always return valid JSON."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.3,  # Lower temperature for more consistent grading
            max_tokens=2048,
            response_format={"type": "json_object"}
        )

        logger.info("Received grading response from Azure OpenAI")

        # Parse response
        response_text = response.choices[0].message.content.strip()
        logger.debug(f"Response: {response_text[:500]}...")

        # Remove markdown if present
        if response_text.startswith("```"):
            response_text = response_text.split("```")[1]
            if response_text.startswith("json"):
                response_text = response_text[4:]
            response_text = response_text.strip()

        # Parse JSON
        try:
            result = json.loads(response_text)
        except json.JSONDecodeError as e:
            logger.error(f"JSON parse error: {e}")
            raise ValueError(f"Failed to parse grading response: {str(e)}")

        # Validate and normalize result
        points_awarded = min(max(int(result.get("points_awarded", 0)), 0), max_points)
        percentage = (points_awarded / max_points * 100) if max_points > 0 else 0

        grading_result = {
            "success": True,
            "points_awarded": points_awarded,
            "max_points": max_points,
            "percentage": round(percentage, 2),
            "feedback": result.get("feedback", ""),
            "strength_points": result.get("strength_points", []),
            "improvement_areas": result.get("improvement_areas", []),
            "detailed_analysis": result.get("detailed_analysis")
        }

        logger.info(f"Grading complete: {points_awarded}/{max_points} ({percentage:.1f}%)")
        return grading_result

    except Exception as e:
        logger.error(f"Error grading answer: {e}", exc_info=True)
        raise ValueError(f"Failed to grade answer: {str(e)}")


async def test_grader():
    """Test the grader with sample data."""
    result = await grade_answer(
        question_content="Explain the difference between a list and a tuple in Python.",
        submitted_answer="A list is mutable, meaning you can change its contents after creation. A tuple is immutable - once created, you cannot modify it. Lists use square brackets [], tuples use parentheses (). Tuples are faster and use less memory.",
        max_points=10,
        grading_rubric='{"criteria": [{"description": "Explains mutability difference", "points": 4}, {"description": "Mentions syntax difference", "points": 3}, {"description": "Mentions performance/memory", "points": 3}]}',
        expected_answer="Lists are mutable (can be modified), tuples are immutable (cannot be modified). Lists use [], tuples use (). Tuples are generally faster and more memory efficient.",
        language="en"
    )
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    import asyncio
    logging.basicConfig(level=logging.DEBUG)
    asyncio.run(test_grader())
