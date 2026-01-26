"""
Answer Grader
Grades student answers using AI (Gemini/OpenAI)
"""

import json
import logging
from typing import Dict, Any, Optional
from datetime import datetime
from openai import AsyncOpenAI

from config.settings import OPENAI_API_KEY, OPENAI_BASE_URL, LLM_MODEL

logger = logging.getLogger(__name__)

# OpenAI client (lazy-loaded)
_client = None

def get_client():
    """Get or create OpenAI client."""
    global _client
    if _client is None:
        _client = AsyncOpenAI(
            api_key=OPENAI_API_KEY,
            base_url=OPENAI_BASE_URL
        )
    return _client

def build_grading_prompt(
    question_content: str,
    question_type: str,
    expected_answer: Optional[str],
    grading_rubric: Optional[str],
    student_answer: str,
    max_points: int,
    language: str
) -> str:
    """
    Build prompt for AI grading.

    Args:
        question_content: The question text
        question_type: Type of question (ShortAnswer, LongAnswer, CodingChallenge)
        expected_answer: Expected/model answer (optional)
        grading_rubric: Grading criteria (optional)
        student_answer: Student's answer to grade
        max_points: Maximum points for this question
        language: Language for feedback (English/Vietnamese)

    Returns:
        Prompt string for AI
    """

    lang_instruction = "Vietnamese" if language.lower() in ["vi", "vietnamese"] else "English"

    prompt = f"""You are an expert educator grading student answers.

QUESTION:
{question_content}

QUESTION TYPE: {question_type}

MAXIMUM POINTS: {max_points}

"""

    if expected_answer:
        prompt += f"""EXPECTED/MODEL ANSWER:
{expected_answer}

"""

    if grading_rubric:
        prompt += f"""GRADING RUBRIC/CRITERIA:
{grading_rubric}

"""

    prompt += f"""STUDENT'S ANSWER:
{student_answer}

---

Please grade this answer and provide DETAILED feedback in {lang_instruction}.

GRADING GUIDELINES:
1. Be fair and objective
2. Consider partial credit for partially correct answers
3. For coding questions:
   - Check if the logic is correct
   - Check if it handles edge cases (empty input, single element, etc.)
   - Check if it returns the correct type/value
   - Explain what the code does vs what it should do
4. Provide constructive feedback that helps the student learn
5. Be SPECIFIC about what's wrong and what's right

SCORING BREAKDOWN (explain each):
- If answer is completely correct: {max_points} points
- If answer is mostly correct with minor issues: {int(max_points * 0.7)}-{max_points - 1} points
- If answer shows understanding but has significant errors: {int(max_points * 0.4)}-{int(max_points * 0.6)} points
- If answer has some correct elements: {int(max_points * 0.2)}-{int(max_points * 0.3)} points
- If answer is completely wrong or irrelevant: 0 points

You MUST respond with a valid JSON object in this exact format:
{{
    "points_awarded": <number between 0 and {max_points}>,
    "percentage": <number between 0 and 100>,
    "is_correct": <true if points_awarded >= {max_points * 0.7}, false otherwise>,
    "feedback": "<overall feedback explaining WHY this score - be specific about what's correct/incorrect>",
    "strength_points": ["<specific strength with example from answer>", ...],
    "improvement_areas": ["<specific issue: what's wrong and how to fix it>", ...],
    "detailed_analysis": "<DETAILED explanation: 1) What the question asks, 2) What the student's answer does, 3) Why points were given/deducted, 4) Specific errors or missing parts if any>"
}}

IMPORTANT REQUIREMENTS:
- points_awarded must be an integer between 0 and {max_points}
- All feedback must be in {lang_instruction}
- feedback field: Explain clearly WHY this score (e.g., "Được 5/{max_points} điểm vì...")
- strength_points: List SPECIFIC things done correctly (e.g., "Sử dụng đúng cú pháp hàm Python")
- improvement_areas: List SPECIFIC errors with explanation (e.g., "Hàm trả về số lớn nhất thay vì số lớn thứ hai - cần loại bỏ max trước khi tìm")
- detailed_analysis: Must explain the score breakdown clearly
- If code is wrong, explain what it actually does vs what it should do
- If answer is empty or completely off-topic, give 0 points
"""

    return prompt


async def grade_answer(
    question_id: str,
    question_content: str,
    question_type: str,
    expected_answer: Optional[str],
    grading_rubric: Optional[str],
    student_answer: str,
    max_points: int,
    language: str = "Vietnamese"
) -> Dict[str, Any]:
    """
    Grade a student's answer using AI.

    Args:
        question_id: Question ID
        question_content: The question text
        question_type: Type of question
        expected_answer: Expected/model answer
        grading_rubric: Grading criteria
        student_answer: Student's answer to grade
        max_points: Maximum points
        language: Language for feedback

    Returns:
        Grading result with score, feedback, and analysis
    """
    logger.info(f"Starting AI grading for question {question_id}")

    # Handle empty answers
    if not student_answer or student_answer.strip() == "":
        logger.warning(f"Empty answer received for question {question_id}")
        no_answer_feedback = "Không có câu trả lời." if language.lower() in ["vi", "vietnamese"] else "No answer provided."
        return {
            "success": True,
            "question_id": question_id,
            "points_awarded": 0,
            "max_points": max_points,
            "percentage": 0.0,
            "is_correct": False,
            "feedback": no_answer_feedback,
            "strength_points": [],
            "improvement_areas": [no_answer_feedback],
            "detailed_analysis": no_answer_feedback,
            "graded_at": datetime.utcnow().isoformat()
        }

    # Build grading prompt
    prompt = build_grading_prompt(
        question_content=question_content,
        question_type=question_type,
        expected_answer=expected_answer,
        grading_rubric=grading_rubric,
        student_answer=student_answer,
        max_points=max_points,
        language=language
    )

    try:
        client = get_client()

        logger.debug(f"Calling AI for grading with model: {LLM_MODEL}")

        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert educator and grader. Always respond with valid JSON only, no markdown formatting."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.3,  # Lower temperature for more consistent grading
            max_tokens=1000
        )

        content = response.choices[0].message.content
        logger.debug(f"AI response: {content[:500]}...")

        # Parse JSON response
        # Clean up response if wrapped in markdown code blocks
        if content.startswith("```"):
            content = content.split("```")[1]
            if content.startswith("json"):
                content = content[4:]
        content = content.strip()

        try:
            result = json.loads(content)
        except json.JSONDecodeError as e:
            logger.error(f"Failed to parse AI response as JSON: {e}")
            logger.error(f"Response content: {content}")
            raise ValueError(f"AI returned invalid JSON: {e}")

        # Validate and normalize result
        points_awarded = min(max(int(result.get("points_awarded", 0)), 0), max_points)
        percentage = (points_awarded / max_points * 100) if max_points > 0 else 0

        return {
            "success": True,
            "question_id": question_id,
            "points_awarded": points_awarded,
            "max_points": max_points,
            "percentage": round(percentage, 1),
            "is_correct": result.get("is_correct", points_awarded >= max_points * 0.7),
            "feedback": result.get("feedback", ""),
            "strength_points": result.get("strength_points", []),
            "improvement_areas": result.get("improvement_areas", []),
            "detailed_analysis": result.get("detailed_analysis", ""),
            "graded_at": datetime.utcnow().isoformat()
        }

    except Exception as e:
        logger.error(f"AI grading failed: {e}", exc_info=True)
        raise ValueError(f"Failed to grade answer: {str(e)}")
