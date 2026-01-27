"""
Skill Gap Analyzer
Analyzes skill gaps and provides AI-generated insights and recommendations
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


def build_gap_analysis_prompt(
    employee_name: str,
    job_role: str,
    skill_name: str,
    skill_code: str,
    current_level: int,
    required_level: int,
    skill_description: Optional[str] = None,
    current_level_description: Optional[str] = None,
    required_level_description: Optional[str] = None,
    language: str = "en"
) -> str:
    """
    Build prompt for analyzing a skill gap.
    """
    lang_name = "English" if language == "en" else "Vietnamese"
    gap_size = required_level - current_level

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
    required_level_name = level_names.get(required_level, f"Level {required_level}")

    prompt = f"""You are an expert HR consultant specializing in skill development and competency frameworks (SFIA).

CONTEXT:
- Employee: {employee_name}
- Current Role: {job_role}
- Skill: {skill_name} ({skill_code})
- Current Level: {current_level} ({current_level_name})
- Required Level: {required_level} ({required_level_name})
- Gap Size: {gap_size} levels

{f'SKILL DESCRIPTION: {skill_description}' if skill_description else ''}
{f'CURRENT LEVEL DESCRIPTION: {current_level_description}' if current_level_description else ''}
{f'REQUIRED LEVEL DESCRIPTION: {required_level_description}' if required_level_description else ''}

TASK:
Analyze this skill gap and provide insights. Consider:
1. Why this gap matters for the role
2. Business impact of not closing the gap
3. Realistic strategies to close the gap
4. Priority and urgency assessment

OUTPUT REQUIREMENTS:
Return ONLY valid JSON matching this exact schema (no markdown, no explanations):

{{
  "ai_analysis": "Detailed analysis of why this gap matters and its impact (2-4 sentences in {lang_name})",
  "ai_recommendation": "Specific, actionable recommendation to close this gap (2-4 sentences in {lang_name})",
  "priority_rationale": "Brief explanation of priority level in {lang_name}",
  "estimated_effort": "Estimated effort to close gap (e.g., '3-6 months with focused training')",
  "key_actions": ["Action 1", "Action 2", "Action 3"],
  "potential_blockers": ["Blocker 1", "Blocker 2"]
}}

IMPORTANT:
- Be specific and actionable
- Consider the gap size when recommending approach
- Write all text fields in {lang_name}
- Return ONLY valid JSON
"""
    return prompt


async def analyze_skill_gap(
    employee_name: str,
    job_role: str,
    skill_name: str,
    skill_code: str,
    current_level: int,
    required_level: int,
    skill_description: Optional[str] = None,
    current_level_description: Optional[str] = None,
    required_level_description: Optional[str] = None,
    language: str = "en"
) -> Dict[str, Any]:
    """
    Analyze a skill gap using Azure OpenAI.

    Returns:
        Dict with AI analysis and recommendations
    """
    logger.info(f"Analyzing gap for {skill_name}: {current_level} -> {required_level}")

    # Handle no gap case
    if current_level >= required_level:
        return {
            "success": True,
            "ai_analysis": "No gap exists - employee meets or exceeds the required level." if language == "en"
                          else "Không có khoảng cách - nhân viên đạt hoặc vượt mức yêu cầu.",
            "ai_recommendation": "Continue to maintain and share expertise with team members." if language == "en"
                                else "Tiếp tục duy trì và chia sẻ kiến thức với các thành viên trong nhóm.",
            "priority_rationale": "No action needed",
            "estimated_effort": "N/A",
            "key_actions": [],
            "potential_blockers": []
        }

    try:
        prompt = build_gap_analysis_prompt(
            employee_name=employee_name,
            job_role=job_role,
            skill_name=skill_name,
            skill_code=skill_code,
            current_level=current_level,
            required_level=required_level,
            skill_description=skill_description,
            current_level_description=current_level_description,
            required_level_description=required_level_description,
            language=language
        )

        logger.debug(f"Gap analysis prompt built: {len(prompt)} characters")

        # Call Azure OpenAI
        client = get_client()
        response = await client.chat.completions.create(
            model=LLM_MODEL,
            messages=[
                {
                    "role": "system",
                    "content": "You are an expert HR consultant specializing in skill development. Provide practical, actionable insights. Always return valid JSON."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            temperature=0.4,
            max_tokens=1024,
            response_format={"type": "json_object"}
        )

        logger.info("Received gap analysis response from Azure OpenAI")

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
            "ai_analysis": result.get("ai_analysis", ""),
            "ai_recommendation": result.get("ai_recommendation", ""),
            "priority_rationale": result.get("priority_rationale", ""),
            "estimated_effort": result.get("estimated_effort", ""),
            "key_actions": result.get("key_actions", []),
            "potential_blockers": result.get("potential_blockers", [])
        }

    except Exception as e:
        logger.error(f"Error analyzing skill gap: {e}", exc_info=True)
        raise ValueError(f"Failed to analyze skill gap: {str(e)}")


async def analyze_multiple_gaps(
    employee_name: str,
    job_role: str,
    gaps: List[Dict[str, Any]],
    language: str = "en"
) -> Dict[str, Any]:
    """
    Analyze multiple skill gaps and provide overall assessment.

    Args:
        employee_name: Employee name
        job_role: Job role name
        gaps: List of gap dicts with skill_name, skill_code, current_level, required_level
        language: Response language

    Returns:
        Dict with individual gap analyses and overall summary
    """
    logger.info(f"Analyzing {len(gaps)} gaps for {employee_name}")

    if not gaps:
        return {
            "success": True,
            "gap_analyses": [],
            "overall_summary": "No skill gaps identified." if language == "en"
                              else "Không xác định được khoảng cách kỹ năng nào.",
            "priority_order": [],
            "recommended_focus_areas": []
        }

    # Analyze each gap
    gap_analyses = []
    for gap in gaps:
        try:
            analysis = await analyze_skill_gap(
                employee_name=employee_name,
                job_role=job_role,
                skill_name=gap.get("skill_name", "Unknown"),
                skill_code=gap.get("skill_code", ""),
                current_level=gap.get("current_level", 0),
                required_level=gap.get("required_level", 1),
                skill_description=gap.get("skill_description"),
                language=language
            )
            gap_analyses.append({
                "skill_id": gap.get("skill_id"),
                "skill_name": gap.get("skill_name"),
                "gap_size": gap.get("required_level", 1) - gap.get("current_level", 0),
                **analysis
            })
        except Exception as e:
            logger.error(f"Failed to analyze gap for {gap.get('skill_name')}: {e}")
            gap_analyses.append({
                "skill_id": gap.get("skill_id"),
                "skill_name": gap.get("skill_name"),
                "success": False,
                "error": str(e)
            })

    # Generate overall summary
    successful_analyses = [g for g in gap_analyses if g.get("success")]
    total_gaps = len(gaps)
    critical_gaps = len([g for g in gaps if g.get("gap_size", 0) >= 3])

    lang = language
    if lang == "en":
        overall_summary = f"Analysis complete for {total_gaps} skill gaps. {critical_gaps} critical gaps identified requiring immediate attention."
    else:
        overall_summary = f"Phân tích hoàn tất cho {total_gaps} khoảng cách kỹ năng. {critical_gaps} khoảng cách nghiêm trọng cần được chú ý ngay."

    # Sort by gap size for priority order
    priority_order = sorted(
        [(g.get("skill_name"), g.get("gap_size", 0)) for g in gap_analyses if g.get("success")],
        key=lambda x: x[1],
        reverse=True
    )

    return {
        "success": True,
        "gap_analyses": gap_analyses,
        "overall_summary": overall_summary,
        "priority_order": [p[0] for p in priority_order[:5]],
        "recommended_focus_areas": [p[0] for p in priority_order[:3]]
    }
