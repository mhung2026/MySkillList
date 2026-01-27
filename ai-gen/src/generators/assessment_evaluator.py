"""
Assessment Evaluator
Evaluates assessment responses and determines CurrentLevel using SFIA framework
Bottom-up consecutive logic: CurrentLevel = highest level L where ALL levels min→L have ≥70% correct
(min = skill's lowest defined SFIA level, NOT always 1)
"""

import logging
from typing import Dict, List, Any, Optional
from collections import defaultdict

logger = logging.getLogger(__name__)

# Minimum threshold for passing a level (70%)
LEVEL_PASS_THRESHOLD = 0.70


def evaluate_assessment(
    assessment_data: Dict[str, Any],
    available_levels: Optional[List[int]] = None
) -> Dict[str, Any]:
    """
    Evaluate assessment responses and determine CurrentLevel.

    Uses bottom-up consecutive logic:
    - Groups responses by target_level
    - Calculates % correct per level
    - CurrentLevel = highest consecutive level with ≥70% correct
      starting from the skill's minimum defined level (NOT always L1)

    Args:
        assessment_data: {
            "skill_id": "uuid",
            "skill_name": "optional",
            "responses": [
                {
                    "question_id": "uuid",
                    "question_type": "MultipleChoice|SituationalJudgment|...",
                    "target_level": 1-7,
                    "is_correct": true/false,  # For objective types
                    "score": 0-100,  # For graded types (ShortAnswer, LongAnswer, etc.)
                    "max_score": 100  # Max possible score
                }
            ]
        }
        available_levels: Optional list of SFIA levels defined for this skill
            (e.g., [4,5,6] for a skill that only exists at L4-L6).
            If not provided, derived from the responses.

    Returns:
        {
            "skill_id": "uuid",
            "skill_name": "optional",
            "current_level": 0-7 (0 if first level not passed),
            "min_defined_level": 1-7 (skill's lowest defined level),
            "level_results": { ... },
            "consecutive_levels_passed": int,
            "highest_level_with_responses": int,
            "total_questions": int,
            "overall_score_percentage": float,
            "evaluation_details": { ... }
        }
    """
    skill_id = assessment_data.get("skill_id")
    skill_name = assessment_data.get("skill_name", "")
    responses = assessment_data.get("responses", [])

    if not responses:
        start_level = min(available_levels) if available_levels and len(available_levels) > 0 else 1
        end_level_val = max(available_levels) if available_levels and len(available_levels) > 0 else 7
        logger.warning("No responses provided for evaluation")
        return {
            "skill_id": skill_id,
            "skill_name": skill_name,
            "current_level": 0,
            "min_defined_level": start_level,
            "max_defined_level": end_level_val,
            "level_results": {},
            "consecutive_levels_passed": 0,
            "highest_level_with_responses": 0,
            "total_questions": 0,
            "overall_score_percentage": 0.0,
            "evaluation_details": {
                "method": "bottom_up_consecutive",
                "threshold": LEVEL_PASS_THRESHOLD * 100,
                "start_level": start_level,
                "end_level": end_level_val,
                "breakdown": [],
                "message": "No responses to evaluate"
            }
        }

    # Group responses by target_level
    level_responses: Dict[int, List[Dict]] = defaultdict(list)
    for response in responses:
        target_level = response.get("target_level", 1)
        if isinstance(target_level, int) and 1 <= target_level <= 7:
            level_responses[target_level].append(response)
        else:
            logger.warning(f"Invalid target_level: {target_level}, defaulting to 1")
            level_responses[1].append(response)

    # Calculate statistics per level
    level_results = {}
    total_correct = 0
    total_questions = len(responses)

    for level in range(1, 8):  # L1 to L7
        level_data = level_responses.get(level, [])
        if not level_data:
            continue

        level_correct = 0
        level_total = len(level_data)

        for resp in level_data:
            question_type = resp.get("question_type", "")

            # For objective question types (is_correct is boolean)
            if "is_correct" in resp:
                if resp["is_correct"]:
                    level_correct += 1
            # For graded question types (score-based)
            elif "score" in resp and "max_score" in resp:
                max_score = resp.get("max_score", 100)
                score = resp.get("score", 0)
                if max_score > 0:
                    # Consider >= 70% of max score as "correct"
                    if score / max_score >= LEVEL_PASS_THRESHOLD:
                        level_correct += 1
            else:
                logger.warning(f"Response missing is_correct or score: {resp}")

        total_correct += level_correct
        percentage = (level_correct / level_total * 100) if level_total > 0 else 0.0
        passed = percentage >= (LEVEL_PASS_THRESHOLD * 100)

        level_results[str(level)] = {
            "total": level_total,
            "correct": level_correct,
            "percentage": round(percentage, 1),
            "passed": passed
        }

    # Determine start/end level for consecutive check
    # Priority: available_levels from DB > levels found in responses > default 1-7
    if available_levels and len(available_levels) > 0:
        start_level = min(available_levels)
        end_level = max(available_levels)
    elif level_responses:
        start_level = min(level_responses.keys())
        end_level = max(level_responses.keys())
    else:
        start_level = 1
        end_level = 7

    logger.info(f"Consecutive check from Level {start_level} to {end_level} "
                f"(available_levels={available_levels}, response_levels={sorted(level_responses.keys()) if level_responses else []})")

    # Determine CurrentLevel using bottom-up consecutive logic
    # Only checks within the skill's defined level range [start_level, end_level]
    current_level = 0
    consecutive_passed = 0
    breakdown = []

    for level in range(start_level, end_level + 1):
        level_key = str(level)
        if level_key not in level_results:
            # No questions at this level - stop consecutive check
            breakdown.append({
                "level": level,
                "status": "no_data",
                "message": f"No questions at Level {level}"
            })
            break

        result = level_results[level_key]
        if result["passed"]:
            current_level = level
            consecutive_passed += 1
            breakdown.append({
                "level": level,
                "status": "passed",
                "percentage": result["percentage"],
                "message": f"Level {level} passed ({result['percentage']}% >= {LEVEL_PASS_THRESHOLD * 100}%)"
            })
        else:
            breakdown.append({
                "level": level,
                "status": "failed",
                "percentage": result["percentage"],
                "message": f"Level {level} failed ({result['percentage']}% < {LEVEL_PASS_THRESHOLD * 100}%)"
            })
            # Stop consecutive check - cannot pass higher levels
            break

    # Calculate overall score
    overall_percentage = (total_correct / total_questions * 100) if total_questions > 0 else 0.0

    # Find highest level with responses
    highest_level_with_responses = max(level_responses.keys()) if level_responses else 0

    return {
        "skill_id": skill_id,
        "skill_name": skill_name,
        "current_level": current_level,
        "min_defined_level": start_level,
        "max_defined_level": end_level,
        "level_results": level_results,
        "consecutive_levels_passed": consecutive_passed,
        "highest_level_with_responses": highest_level_with_responses,
        "total_questions": total_questions,
        "overall_score_percentage": round(overall_percentage, 1),
        "evaluation_details": {
            "method": "bottom_up_consecutive",
            "threshold": LEVEL_PASS_THRESHOLD * 100,
            "start_level": start_level,
            "end_level": end_level,
            "breakdown": breakdown
        }
    }


def evaluate_multiple_skills(
    assessments: List[Dict[str, Any]]
) -> Dict[str, Any]:
    """
    Evaluate multiple skill assessments at once.

    Args:
        assessments: List of assessment_data objects (see evaluate_assessment)

    Returns:
        {
            "results": [
                { evaluation result for skill 1 },
                { evaluation result for skill 2 },
                ...
            ],
            "summary": {
                "total_skills": 3,
                "skills_evaluated": 3,
                "average_level": 2.5
            }
        }
    """
    results = []
    total_levels = 0
    skills_with_level = 0

    for assessment_data in assessments:
        result = evaluate_assessment(assessment_data)
        results.append(result)

        if result["current_level"] > 0:
            total_levels += result["current_level"]
            skills_with_level += 1

    average_level = round(total_levels / skills_with_level, 1) if skills_with_level > 0 else 0

    return {
        "results": results,
        "summary": {
            "total_skills": len(assessments),
            "skills_evaluated": skills_with_level,
            "average_level": average_level
        }
    }
