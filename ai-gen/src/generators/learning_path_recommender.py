"""
Learning Path Recommender
AI-powered learning path generation and resource recommendations
"""

import json
import logging
from typing import Dict, Any, List, Optional
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


def build_learning_path_prompt(
    employee_name: str,
    skill_name: str,
    skill_code: str,
    current_level: int,
    target_level: int,
    skill_description: Optional[str] = None,
    available_resources: Optional[List[Dict[str, Any]]] = None,
    time_constraint_months: Optional[int] = None,
    language: str = "en"
) -> str:
    """
    Build prompt for generating a learning path.
    """
    lang_name = "English" if language == "en" else "Vietnamese"

    # SFIA level names
    level_names = {
        0: "None",
        1: "Follow",
        2: "Assist",
        3: "Apply",
        4: "Enable",
        5: "Ensure/Advise",
        6: "Initiate",
        7: "Set Strategy"
    }

    current_level_name = level_names.get(current_level, f"Level {current_level}")
    target_level_name = level_names.get(target_level, f"Level {target_level}")

    # Format available resources if provided
    resources_text = ""
    if available_resources and len(available_resources) > 0:
        resources_text = "\nAVAILABLE LEARNING RESOURCES (use these REAL courses in your learning path when relevant):\n"
        for i, res in enumerate(available_resources[:15], 1):
            source = res.get('source', '')
            source_tag = f" [{source}]" if source else ""
            org = res.get('organization', '')
            org_tag = f" by {org}" if org else ""
            rating = res.get('rating')
            rating_tag = f" | Rating: {rating}/5" if rating else ""
            hours = res.get('estimated_hours', '?')
            level = res.get('difficulty', '')
            level_tag = f" | Level: {level}" if level else ""

            resources_text += f"{i}. {res.get('title', 'Untitled')}{source_tag}{org_tag} ({hours} hours{level_tag}{rating_tag})\n"
            resources_text += f"   ID: {res.get('id', '')}\n"
            if res.get('url'):
                resources_text += f"   URL: {res['url']}\n"
            if res.get('description'):
                resources_text += f"   Description: {res['description'][:200]}\n"
            if res.get('syllabus') and isinstance(res['syllabus'], list):
                syllabus_preview = ", ".join(str(s) for s in res['syllabus'][:5])
                resources_text += f"   Syllabus: {syllabus_preview}\n"

    time_text = ""
    if time_constraint_months:
        time_text = f"\nTIME CONSTRAINT: Complete within {time_constraint_months} months"

    prompt = f"""You are an expert learning and development consultant specializing in IT/Tech skill development.

CONTEXT:
- Employee: {employee_name}
- Skill to Develop: {skill_name} ({skill_code})
- Current Level: {current_level} ({current_level_name})
- Target Level: {target_level} ({target_level_name})
- Levels to Advance: {target_level - current_level}
{f'SKILL DESCRIPTION: {skill_description}' if skill_description else ''}
{resources_text}
{time_text}

TASK:
Create a comprehensive learning path to help the employee advance from level {current_level} to level {target_level}.

Consider:
1. Progressive skill building (don't skip levels)
2. Mix of learning methods (courses, projects, mentoring, etc.)
3. Practical application opportunities
4. Milestones to measure progress

OUTPUT REQUIREMENTS:
Return ONLY valid JSON matching this exact schema (no markdown):

{{
  "path_title": "Title for this learning path in {lang_name}",
  "path_description": "Brief description of the learning journey in {lang_name}",
  "estimated_total_hours": <number>,
  "estimated_duration_weeks": <number>,
  "learning_items": [
    {{
      "order": 1,
      "title": "Learning item title in {lang_name}",
      "description": "What will be learned in {lang_name}",
      "item_type": "Project|Mentorship|Workshop|Certification|Book|Article",
      "estimated_hours": <number>,
      "target_level_after": <number 1-7>,
      "success_criteria": "How to know this is complete in {lang_name}",
      "resource_id": "<id if matching available resource, null otherwise>",
      "url": "<URL if available resource has one, null otherwise>"
    }}
  ],
  "milestones": [
    {{
      "after_item": <order number>,
      "description": "Milestone description in {lang_name}",
      "expected_level": <number>
    }}
  ],
  "ai_rationale": "Explanation of why this path was designed this way in {lang_name}",
  "key_success_factors": ["Factor 1", "Factor 2"],
  "potential_challenges": ["Challenge 1", "Challenge 2"]
}}

IMPORTANT:
- Create 3-6 learning items depending on gap size
- Each level advancement typically needs 20-40 hours of learning
- Include at least one hands-on project
- ONLY generate non-course items: Project, Mentorship, Workshop, Certification, Book, Article
- Do NOT generate any Course items. Real courses will be added separately.
- item_type must be one of: Project, Mentorship, Workshop, Certification, Book, Article
- All text in {lang_name}
- Return ONLY valid JSON
"""
    return prompt


async def generate_learning_path(
    employee_name: str,
    skill_name: str,
    skill_code: str,
    current_level: int,
    target_level: int,
    skill_description: Optional[str] = None,
    available_resources: Optional[List[Dict[str, Any]]] = None,
    time_constraint_months: Optional[int] = None,
    language: str = "en"
) -> Dict[str, Any]:
    """
    Generate a learning path using Azure OpenAI.

    Returns:
        Dict with learning path details
    """
    logger.info(f"Generating learning path for {skill_name}: {current_level} -> {target_level}")

    # Validate input
    if current_level >= target_level:
        return {
            "success": True,
            "path_title": f"Maintain {skill_name} Expertise",
            "path_description": "No advancement needed - already at or above target level.",
            "estimated_total_hours": 0,
            "estimated_duration_weeks": 0,
            "learning_items": [],
            "milestones": [],
            "ai_rationale": "Employee already meets the target level.",
            "key_success_factors": [],
            "potential_challenges": []
        }

    try:
        prompt = build_learning_path_prompt(
            employee_name=employee_name,
            skill_name=skill_name,
            skill_code=skill_code,
            current_level=current_level,
            target_level=target_level,
            skill_description=skill_description,
            available_resources=available_resources,
            time_constraint_months=time_constraint_months,
            language=language
        )

        logger.debug(f"Learning path prompt built: {len(prompt)} characters")

        # Call Azure OpenAI
        client = get_client()
        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert learning and development consultant. Design effective, practical learning paths. Always return valid JSON."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.5,
            max_tokens=2048,
            response_format={"type": "json_object"}
        )

        logger.info("Received learning path response from Azure OpenAI")

        # Parse response
        response_text = response.choices[0].message.content.strip()

        # Remove markdown if present
        if response_text.startswith("```"):
            response_text = response_text.split("```")[1]
            if response_text.startswith("json"):
                response_text = response_text[4:]
            response_text = response_text.strip()

        result = json.loads(response_text)

        return {
            "success": True,
            "path_title": result.get("path_title", ""),
            "path_description": result.get("path_description", ""),
            "estimated_total_hours": result.get("estimated_total_hours", 0),
            "estimated_duration_weeks": result.get("estimated_duration_weeks", 0),
            "learning_items": result.get("learning_items", []),
            "milestones": result.get("milestones", []),
            "ai_rationale": result.get("ai_rationale", ""),
            "key_success_factors": result.get("key_success_factors", []),
            "potential_challenges": result.get("potential_challenges", [])
        }

    except Exception as e:
        logger.error(f"Error generating learning path: {e}", exc_info=True)
        raise ValueError(f"Failed to generate learning path: {str(e)}")


def build_multiple_learning_paths_prompt(
    employee_name: str,
    skills: List[Dict[str, Any]],
    time_constraint_months: Optional[int] = None,
    language: str = "en"
) -> str:
    """
    Build prompt for generating learning paths for multiple skills in a single AI call.
    """
    lang_name = "English" if language == "en" else "Vietnamese"

    level_names = {
        0: "None", 1: "Follow", 2: "Assist", 3: "Apply",
        4: "Enable", 5: "Ensure/Advise", 6: "Initiate", 7: "Set Strategy"
    }

    # Build skills context
    skills_text = ""
    for i, s in enumerate(skills, 1):
        cur = s.get("current_level", 0)
        tgt = s.get("target_level", 1)
        cur_name = level_names.get(cur, f"Level {cur}")
        tgt_name = level_names.get(tgt, f"Level {tgt}")
        desc = f'\n   Description: {s["skill_description"]}' if s.get("skill_description") else ""
        skills_text += f"""
{i}. {s['skill_name']} ({s['skill_code']})
   Current Level: {cur} ({cur_name}) → Target Level: {tgt} ({tgt_name})
   Gap: {tgt - cur} level(s){desc}
"""

    time_text = ""
    if time_constraint_months:
        time_text = f"\nTIME CONSTRAINT: Complete all paths within {time_constraint_months} months"

    prompt = f"""You are an expert learning and development consultant specializing in IT/Tech skill development.

CONTEXT:
- Employee: {employee_name}
- Number of skills to develop: {len(skills)}
{time_text}

SKILLS TO DEVELOP:
{skills_text}

TASK:
Create a comprehensive learning path for EACH skill listed above. Also provide an overall summary analyzing cross-skill priorities and a recommended learning order.

For EACH skill, consider:
1. Progressive skill building (don't skip levels)
2. Mix of learning methods (projects, mentoring, workshops, etc.)
3. Practical application opportunities
4. Milestones to measure progress

OUTPUT REQUIREMENTS:
Return ONLY valid JSON matching this exact schema (no markdown):

{{
  "learning_paths": [
    {{
      "skill_code": "<skill code>",
      "path_title": "Title for this learning path in {lang_name}",
      "path_description": "Brief description in {lang_name}",
      "estimated_total_hours": <number>,
      "estimated_duration_weeks": <number>,
      "learning_items": [
        {{
          "order": 1,
          "title": "Learning item title in {lang_name}",
          "description": "What will be learned in {lang_name}",
          "item_type": "Project|Mentorship|Workshop|Certification|Book|Article",
          "estimated_hours": <number>,
          "target_level_after": <number 1-7>,
          "success_criteria": "How to know this is complete in {lang_name}"
        }}
      ],
      "milestones": [
        {{
          "after_item": <order number>,
          "description": "Milestone description in {lang_name}",
          "expected_level": <number>
        }}
      ],
      "ai_rationale": "Explanation of why this path was designed this way in {lang_name}",
      "key_success_factors": ["Factor 1", "Factor 2"],
      "potential_challenges": ["Challenge 1", "Challenge 2"]
    }}
  ],
  "overall_summary": "Cross-skill analysis: priorities, synergies between skills, overall development strategy in {lang_name}",
  "recommended_learning_order": ["SKILL_CODE_1", "SKILL_CODE_2"]
}}

IMPORTANT:
- Generate ONE learning_paths entry per skill, matched by skill_code
- Create 3-6 learning items per skill depending on gap size
- Each level advancement typically needs 20-40 hours of learning
- Include at least one hands-on project per skill
- ONLY generate non-course items: Project, Mentorship, Workshop, Certification, Book, Article
- Do NOT generate any Course items. Real courses will be added separately.
- item_type must be one of: Project, Mentorship, Workshop, Certification, Book, Article
- recommended_learning_order: sort skill_codes by priority (learn first → learn last)
- overall_summary: analyze synergies, dependencies, and prioritization across all skills
- All text in {lang_name}
- Return ONLY valid JSON
"""
    return prompt


async def generate_multiple_learning_paths(
    employee_name: str,
    skills: List[Dict[str, Any]],
    time_constraint_months: Optional[int] = None,
    language: str = "en"
) -> Dict[str, Any]:
    """
    Generate learning paths for multiple skills in a single AI call.

    Args:
        employee_name: Employee name
        skills: List of dicts with skill_name, skill_code, current_level, target_level, skill_description
        time_constraint_months: Optional time constraint
        language: Response language (en/vi)

    Returns:
        Dict with learning_paths[], overall_summary, recommended_learning_order
    """
    logger.info(f"Generating learning paths for {len(skills)} skills for {employee_name}")

    if not skills:
        return {
            "success": True,
            "learning_paths": [],
            "overall_summary": "No skills provided." if language == "en" else "Không có kỹ năng nào được cung cấp.",
            "recommended_learning_order": []
        }

    try:
        prompt = build_multiple_learning_paths_prompt(
            employee_name=employee_name,
            skills=skills,
            time_constraint_months=time_constraint_months,
            language=language
        )

        logger.debug(f"Multi-learning-path prompt built: {len(prompt)} characters")

        client = get_client()
        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert learning and development consultant. Design effective, practical learning paths for multiple skills. Always return valid JSON."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.5,
            max_tokens=4096,
            response_format={"type": "json_object"}
        )

        logger.info("Received multi-learning-path response from Azure OpenAI")

        response_text = response.choices[0].message.content.strip()

        if response_text.startswith("```"):
            response_text = response_text.split("```")[1]
            if response_text.startswith("json"):
                response_text = response_text[4:]
            response_text = response_text.strip()

        result = json.loads(response_text)

        # Normalize each path to match single endpoint field structure
        learning_paths = []
        for path in result.get("learning_paths", []):
            learning_paths.append({
                "skill_code": path.get("skill_code", ""),
                "path_title": path.get("path_title", ""),
                "path_description": path.get("path_description", ""),
                "estimated_total_hours": path.get("estimated_total_hours", 0),
                "estimated_duration_weeks": path.get("estimated_duration_weeks", 0),
                "learning_items": path.get("learning_items", []),
                "milestones": path.get("milestones", []),
                "ai_rationale": path.get("ai_rationale", ""),
                "key_success_factors": path.get("key_success_factors", []),
                "potential_challenges": path.get("potential_challenges", [])
            })

        return {
            "success": True,
            "learning_paths": learning_paths,
            "overall_summary": result.get("overall_summary", ""),
            "recommended_learning_order": result.get("recommended_learning_order", [])
        }

    except Exception as e:
        logger.error(f"Error generating multiple learning paths: {e}", exc_info=True)
        raise ValueError(f"Failed to generate multiple learning paths: {str(e)}")


async def rank_learning_resources(
    skill_name: str,
    skill_code: str,
    current_level: int,
    target_level: int,
    resources: List[Dict[str, Any]],
    language: str = "en"
) -> Dict[str, Any]:
    """
    Rank learning resources by relevance for a specific skill gap.

    Args:
        skill_name: Name of the skill
        skill_code: Code of the skill
        current_level: Current proficiency level
        target_level: Target proficiency level
        resources: List of available resources with title, type, description, etc.
        language: Response language

    Returns:
        Dict with ranked resources and recommendations
    """
    logger.info(f"Ranking {len(resources)} resources for {skill_name}")

    if not resources:
        return {
            "success": True,
            "ranked_resources": [],
            "top_recommendations": [],
            "coverage_assessment": "No resources available to rank.",
            "gaps_in_resources": []
        }

    lang_name = "English" if language == "en" else "Vietnamese"

    # Format resources for prompt
    resources_json = json.dumps([
        {
            "id": r.get("id", str(i)),
            "title": r.get("title", ""),
            "type": r.get("type", ""),
            "description": r.get("description", "")[:200] if r.get("description") else "",
            "estimated_hours": r.get("estimated_hours"),
            "difficulty": r.get("difficulty", ""),
            "from_level": r.get("from_level"),
            "to_level": r.get("to_level")
        }
        for i, r in enumerate(resources[:20])  # Limit to 20
    ], indent=2)

    prompt = f"""You are an expert learning consultant. Rank these learning resources by relevance.

SKILL DEVELOPMENT CONTEXT:
- Skill: {skill_name} ({skill_code})
- Current Level: {current_level}
- Target Level: {target_level}
- Gap: {target_level - current_level} levels

AVAILABLE RESOURCES:
{resources_json}

TASK:
Rank these resources by how well they help close the skill gap. Consider:
1. Relevance to the specific skill
2. Appropriateness for the current -> target level transition
3. Learning efficiency (hours vs. value)
4. Logical learning sequence

OUTPUT REQUIREMENTS:
Return ONLY valid JSON:

{{
  "ranked_resources": [
    {{
      "resource_id": "<id>",
      "rank": 1,
      "relevance_score": <0-100>,
      "reason": "Why this resource is ranked here in {lang_name}"
    }}
  ],
  "top_recommendations": ["id1", "id2", "id3"],
  "coverage_assessment": "How well these resources cover the skill gap in {lang_name}",
  "gaps_in_resources": ["What's missing in {lang_name}"]
}}
"""

    try:
        client = get_client()
        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert learning consultant. Evaluate and rank learning resources objectively. Always return valid JSON."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.3,
            max_tokens=1024,
            response_format={"type": "json_object"}
        )

        response_text = response.choices[0].message.content.strip()

        if response_text.startswith("```"):
            response_text = response_text.split("```")[1]
            if response_text.startswith("json"):
                response_text = response_text[4:]
            response_text = response_text.strip()

        result = json.loads(response_text)

        return {
            "success": True,
            "ranked_resources": result.get("ranked_resources", []),
            "top_recommendations": result.get("top_recommendations", []),
            "coverage_assessment": result.get("coverage_assessment", ""),
            "gaps_in_resources": result.get("gaps_in_resources", [])
        }

    except Exception as e:
        logger.error(f"Error ranking resources: {e}", exc_info=True)
        raise ValueError(f"Failed to rank resources: {str(e)}")
