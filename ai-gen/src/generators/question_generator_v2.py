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
    target_proficiency_level_raw = normalized_request.get("target_proficiency_level", None)

    # Normalize target_proficiency_level: schema defines as List[int] but generator needs single int (max)
    if isinstance(target_proficiency_level_raw, list) and len(target_proficiency_level_raw) > 0:
        target_proficiency_levels = max(target_proficiency_level_raw)
    elif isinstance(target_proficiency_level_raw, int):
        target_proficiency_levels = target_proficiency_level_raw
    else:
        target_proficiency_levels = None

    # Language mapping
    lang_name = "English" if language == "en" else "Vietnamese"

    # Build skill context
    skill_context = ""
    skill_id_for_prompt = None

    if skill_data:
        skill_name = skill_data.get("skill_name", "")
        skill_id_for_prompt = skill_data.get("skill_id")
        skill_code = skill_data.get("skill_code", "")
        levels = skill_data.get("levels", [])

        skill_context = f"""
SKILL: {skill_name}
SKILL_CODE: {skill_code}
SKILL_ID: {skill_id_for_prompt}

PROFICIENCY LEVELS (SFIA Framework 1-7):
"""
        for level_data in levels:
            level_num = level_data["level"]
            desc = level_data.get("description", "")
            autonomy = level_data.get("autonomy", "")
            influence = level_data.get("influence", "")
            complexity = level_data.get("complexity", "")
            business_skills = level_data.get("business_skills", "")
            knowledge = level_data.get("knowledge", "")
            behavioral_indicators = level_data.get("behavioral_indicators", "")
            evidence_examples = level_data.get("evidence_examples", "")

            skill_context += f"""
=== Level {level_num} ===
Description: {desc}
Autonomy: {autonomy}
Influence: {influence}
Complexity: {complexity}
Business Skills: {business_skills}
Knowledge Required: {knowledge}
Behavioral Indicators: {behavioral_indicators}
Evidence Examples: {evidence_examples}
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

    # Calculate type distribution
    num_types = len(question_types)
    base_per_type = num_questions // num_types
    remainder = num_questions % num_types

    # Build question type instructions with distribution
    type_instructions = f"""
QUESTION TYPES TO GENERATE ({num_questions} questions total across {num_types} types):
Distribution: Generate approximately {base_per_type} question(s) per type{f", with {remainder} extra distributed among types" if remainder > 0 else ""}.
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
            type_instructions += """  * FORCED-CHOICE BEHAVIORAL ASSESSMENT (SJT)
  * FIXED CONTEXT - Scenario MUST explicitly define:
    - Task type (bug fix, feature, analysis, decision, coordination)
    - Constraints (deadline, documentation availability, risk level)
    - Authority conditions (leader available/unavailable)
    - Scope clarity (clear/ambiguous)
  * INSTRUCTION: "Choose the action you are MOST LIKELY to take in real life"
  * EXACTLY 4 OPTIONS - Each option MUST:
    - Be realistic and defensible (no obviously good/bad answers)
    - Represent ONE dominant behavior dimension:
      * Dependency (waiting for approval, close supervision)
      * Autonomy (acting within scope, self-direction)
      * Risk avoidance (minimizing exposure)
      * Speed bias (acting before validation)
      * Peer reliance (seeking consensus)
    - Map to a SINGLE SFIA level:
      * L1: Requires close supervision
      * L2: Follows instructions, seeks confirmation
      * L3: Works independently within defined scope
      * L4: Takes responsibility for approach and quality
  * TRADE-OFF ENFORCEMENT - Question MUST force trade-off between:
    - Autonomy ↔ Safety
    - Speed ↔ Quality
    - Responsibility ↔ Escalation
    - Initiative ↔ Compliance
  * effectiveness_level maps to SFIA: MostEffective=L4, Effective=L3, Ineffective=L2, CounterProductive=L1
"""
        elif qtype == "Rating":
            type_instructions += "  * 5 rating scale options, all marked as correct\n"

    # Add distribution reminder
    allowed_types_str = ", ".join(question_types)
    type_instructions += f"""
TYPE DISTRIBUTION RULE:
- Total questions required: {num_questions}
- Number of types: {num_types}
- ⚠️ ONLY USE THESE EXACT TYPES: [{allowed_types_str}]
- DO NOT generate any other question types
- Distribute questions across ALL listed types
- Each type should have at least 1 question if possible
- Vary the distribution naturally (e.g., for 5 questions with 3 types: 2-2-1 or 2-1-2)
"""

    # Build level targeting instructions based on target_proficiency_level
    if target_proficiency_levels is not None and target_proficiency_levels > 0:
        max_level = min(target_proficiency_levels, 7)  # Cap at 7
        levels_list = list(range(1, max_level + 1))
        levels_str = ", ".join(str(l) for l in levels_list)

        # Calculate minimum questions per level for coverage
        min_per_level = max(1, num_questions // max_level)

        level_targeting_instructions = f"""LEVEL TARGETING (MANDATORY):
⚠️ TARGET PROFICIENCY LEVEL: {max_level}
⚠️ ALLOWED LEVELS: Only generate questions with target_level in [{levels_str}]
⚠️ DO NOT generate any question with target_level > {max_level}

LEVEL COVERAGE REQUIREMENT:
- You MUST cover ALL levels from 1 to {max_level}
- Each level MUST have at least 1 question
- Distribute {num_questions} questions across {max_level} levels
- Suggested minimum per level: {min_per_level} question(s)

LEVEL BEHAVIORAL SIGNATURES:
- L1 (Follow): Waits for instruction, avoids decisions, requires close supervision
- L2 (Assist): Follows guidance, seeks confirmation, works under routine direction
- L3 (Apply): Acts independently within defined scope, uses discretion
- L4 (Enable): Owns approach and quality, substantial responsibility
- L5 (Ensure/Advise): Influences others, ensures consistency across organization
- L6 (Initiate/Influence): Initiates change under uncertainty, significant responsibility
- L7 (Set Strategy): Sets long-term direction and vision, organization-wide impact

For this assessment targeting Level {max_level}:
- Include foundational questions at L1-L2 (basic competency verification)
- Include core questions at L3-L4 (independent work capability)
{"- Include advanced questions at L5-" + str(max_level) + " (leadership/strategic capability)" if max_level >= 5 else ""}
"""
    else:
        # Default behavior when no target level specified - allow all levels
        level_targeting_instructions = """LEVEL TARGETING:
- L1-2: Scenarios requiring guidance, following instructions, seeking confirmation
- L3-4: Scenarios requiring independent judgment within defined scope
- L5-7: Scenarios requiring strategic decisions, organizational impact, leading others

You may generate questions at any level from 1-7 based on the skill context and difficulty."""

    prompt = f"""You are an expert assessment question generator specializing in SFIA (Skills Framework for the Information Age) competency assessments.

{skill_context}

{additional_context_text}

TASK:
Generate EXACTLY {num_questions} assessment questions in {lang_name}.
⚠️ QUANTITY REQUIREMENT: You MUST generate exactly {num_questions} questions - no more, no less. This is mandatory.

CRITICAL INSTRUCTIONS - BEHAVIOR-BASED ASSESSMENT:

CORE PRINCIPLE: Measure what the person is LIKELY TO DO under responsibility, NOT what they know is correct.

AVOID in all questions:
- Moral language ("you should", "it's wrong to")
- "Best practice" framing
- Knowledge recall (definitions, theory)
- Obvious good/bad answers
- Socially desirable answer patterns

USE skill knowledge as follows:
1. "Behavioral Indicators" → craft realistic workplace scenarios
2. "Evidence Examples" → create authentic situational contexts
3. "Autonomy/Influence/Complexity" → calibrate decision scope per level
4. "Knowledge Required" → inform scenario background, NOT test recall

{level_targeting_instructions}

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
1. ⚠️ MANDATORY: Generate EXACTLY {num_questions} questions. Count them before responding.
2. Use the exact skill_id provided above for ALL questions (do not generate random UUIDs)
3. For MultipleChoice: Exactly 1 option with is_correct=true
4. For MultipleAnswer: 2+ options with is_correct=true
5. For TrueFalse: Exactly 2 options (True/False)
6. For ShortAnswer/LongAnswer: Include grading_rubric as JSON string
7. For CodingChallenge: Include code_snippet and grading_rubric with test_cases
8. For SituationalJudgment: 4 options with effectiveness_level, each representing distinct behavioral strategy mapped to SFIA level
9. For Rating: 3-5 options, all with is_correct=true
10. Use clear, professional language
11. Ensure questions are at appropriate difficulty level
12. Return ONLY the JSON, no markdown blocks or explanations
13. ⚠️ LEVEL COVERAGE: If target_proficiency_level is specified, ensure ALL levels from 1 to that level are covered with at least 1 question each

Generate questions that are:
- Behavior-revealing: expose what candidates actually DO, not what they know
- Realistic: authentic workplace scenarios with concrete constraints
- Defensible options: every choice is rational in some context
- Trade-off driven: force meaningful decisions between competing values
- Level-differentiated: options map clearly to SFIA behavioral signatures
- Unambiguous: clear context with defined constraints

FINAL CHECK:
1. Your response MUST contain exactly {num_questions} question objects in the "questions" array
2. Verify level coverage: all required levels are represented
3. Verify type coverage: all requested question types are used
"""

    return prompt


async def generate_questions_v2(
    normalized_request: Dict[str, Any],
    skill_data: Optional[Dict[str, Any]] = None
) -> Dict[str, Any]:
    """
    Generate questions using Azure OpenAI with V2 schema.
    Implements retry logic to ensure exact question count.

    Args:
        normalized_request: Validated request from request_validator
        skill_data: Optional skill data from database

    Returns:
        Dict with 'questions' and 'metadata' matching output_question_schema_v2
    """
    requested_count = normalized_request["number_of_questions"]
    logger.info(f"Starting AI question generation: {requested_count} questions requested")
    logger.debug(f"Request: {normalized_request}")

    all_questions = []
    max_attempts = 3
    attempt = 0

    try:
        while len(all_questions) < requested_count and attempt < max_attempts:
            attempt += 1
            remaining = requested_count - len(all_questions)

            # Adjust request for remaining questions
            adjusted_request = normalized_request.copy()
            adjusted_request["number_of_questions"] = remaining

            logger.info(f"Attempt {attempt}/{max_attempts}: Generating {remaining} questions")

            # Build prompt
            prompt = build_prompt_v2(adjusted_request, skill_data)
            logger.debug(f"Prompt built: {len(prompt)} characters")

            # Calculate max_tokens - be generous
            # SJT questions are ~600-1000 tokens each
            estimated_tokens = remaining * 1000 + 1000  # 1000 per question + buffer
            max_tokens = min(max(estimated_tokens, 8192), 16000)

            client = get_client()
            response = await client.chat.completions.create(
                model=LLM_MODEL,
                messages=[
                    {
                        "role": "system",
                        "content": f"You are an expert assessment question generator. Return valid JSON only. CRITICAL: Generate EXACTLY {remaining} questions - count them before responding."
                    },
                    {
                        "role": "user",
                        "content": prompt
                    }
                ],
                temperature=0.7,
                max_tokens=max_tokens,
                response_format={"type": "json_object"}
            )

            logger.info("Received response from Azure OpenAI")

            # Parse response
            response_text = response.choices[0].message.content.strip()
            logger.debug(f"Response length: {len(response_text)} characters")

            # Remove markdown code blocks if present
            if response_text.startswith("```"):
                logger.debug("Removing markdown code blocks")
                response_text = response_text.split("```")[1]
                if response_text.startswith("json"):
                    response_text = response_text[4:]
                response_text = response_text.strip()

            # Parse JSON
            try:
                result = json.loads(response_text)
            except json.JSONDecodeError as e:
                logger.error(f"JSON parse error: {e}")
                logger.error(f"Response text: {response_text[:500]}...")
                if attempt < max_attempts:
                    logger.info("Retrying due to JSON parse error...")
                    continue
                raise ValueError(f"Failed to parse AI response as JSON: {str(e)}")

            # Validate structure
            if "questions" not in result:
                logger.warning("Response missing 'questions' key, attempting to wrap")
                if isinstance(result, list):
                    result = {"questions": result}
                else:
                    if attempt < max_attempts:
                        logger.info("Retrying due to invalid structure...")
                        continue
                    raise ValueError("Response does not contain 'questions' array")

            batch_questions = result["questions"]
            logger.info(f"Attempt {attempt}: Got {len(batch_questions)} questions (needed {remaining})")

            # Filter out questions with invalid types
            allowed_types = set(adjusted_request["question_type"])
            valid_questions = []
            invalid_count = 0
            for q in batch_questions:
                q_type = q.get("type", "")
                if q_type in allowed_types:
                    valid_questions.append(q)
                else:
                    invalid_count += 1
                    logger.warning(f"Filtered out question with invalid type: '{q_type}' (allowed: {allowed_types})")

            if invalid_count > 0:
                logger.info(f"Filtered {invalid_count} questions with wrong types, kept {len(valid_questions)}")

            # Filter out questions with target_level exceeding target_proficiency_level
            target_proficiency_raw = adjusted_request.get("target_proficiency_level")
            # Normalize: could be List[int] or int
            if isinstance(target_proficiency_raw, list) and len(target_proficiency_raw) > 0:
                target_proficiency = max(target_proficiency_raw)
            elif isinstance(target_proficiency_raw, int):
                target_proficiency = target_proficiency_raw
            else:
                target_proficiency = None

            if target_proficiency is not None and target_proficiency > 0:
                max_allowed_level = min(target_proficiency, 7)
                level_filtered = []
                level_invalid_count = 0
                for q in valid_questions:
                    q_level = q.get("target_level", 1)
                    if isinstance(q_level, int) and 1 <= q_level <= max_allowed_level:
                        level_filtered.append(q)
                    else:
                        level_invalid_count += 1
                        logger.warning(f"Filtered out question with target_level={q_level} (max allowed: {max_allowed_level})")

                if level_invalid_count > 0:
                    logger.info(f"Filtered {level_invalid_count} questions exceeding target level, kept {len(level_filtered)}")
                valid_questions = level_filtered

            # Add to collection
            all_questions.extend(valid_questions)

            # If we got enough, break
            if len(all_questions) >= requested_count:
                break

        # Trim if we got too many
        if len(all_questions) > requested_count:
            logger.info(f"Trimming from {len(all_questions)} to {requested_count} questions")
            all_questions = all_questions[:requested_count]

        logger.info(f"Final result: {len(all_questions)} questions (requested: {requested_count})")

        if len(all_questions) < requested_count:
            logger.warning(f"Could not generate enough questions after {max_attempts} attempts")

        # Ensure each question has skill_id from request
        skill_id_from_request = None
        if skill_data and "skill_id" in skill_data:
            skill_id_from_request = skill_data["skill_id"]
        elif normalized_request.get("skills") and len(normalized_request["skills"]) > 0:
            skill_id_from_request = normalized_request["skills"][0].get("skill_id")

        # Inject skill_id into each question if not already present or if null
        if skill_id_from_request:
            for question in all_questions:
                if not question.get("skill_id") or question.get("skill_id") == "null":
                    question["skill_id"] = skill_id_from_request
                    logger.debug(f"Injected skill_id {skill_id_from_request} into question")

        # Add metadata
        metadata = {
            "total_questions": len(all_questions),
            "requested_questions": requested_count,
            "generation_attempts": attempt,
            "generation_timestamp": datetime.now().isoformat(),
            "ai_model": LLM_MODEL,
            "skill_id": skill_data.get("skill_id") if skill_data else None,
            "skill_name": skill_data.get("skill_name") if skill_data else None,
            "language": normalized_request["language"],
            "target_proficiency_level": normalized_request.get("target_proficiency_level")
        }

        # Return V2 format
        output = {
            "questions": all_questions,
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
