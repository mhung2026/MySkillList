#!/usr/bin/env python3
"""
Database reader for SkillLevelDefinitions table.
Provides functions to query and retrieve skill level definition data from PostgreSQL.
"""

from src.custom import getConn, LOGGER


def getSkillLevelDefinitions(skill_id=None, level=None):
    """
    Get skill level definitions from database.

    Args:
        skill_id (str, optional): Filter by specific skill ID
        level (int, optional): Filter by specific proficiency level (1-7)

    Returns:
        list: List of tuples containing skill level definition data
    """
    with getConn() as conn:
        with conn.cursor() as cur:
            # Build query dynamically based on filters
            query = """
                SELECT
                    sld."Id",
                    sld."SkillId",
                    s."Name" as "SkillName",
                    s."Code" as "SkillCode",
                    sld."Level",
                    sld."Description",
                    sld."Autonomy",
                    sld."Influence",
                    sld."Complexity",
                    sld."BusinessSkills",
                    sld."Knowledge",
                    sld."BehavioralIndicators",
                    sld."EvidenceExamples",
                    sld."IsDeleted",
                    sld."CreatedAt",
                    sld."UpdatedAt"
                FROM public."SkillLevelDefinitions" sld
                JOIN public."Skills" s ON sld."SkillId" = s."Id"
                WHERE NOT sld."IsDeleted"
                    AND NOT s."IsDeleted"
                    AND s."IsActive" = true
            """

            params = []
            if skill_id:
                query += ' AND sld."SkillId" = %s'
                params.append(skill_id)

            if level is not None:
                query += ' AND sld."Level" = %s'
                params.append(level)

            query += ' ORDER BY s."Name", sld."Level"'

            cur.execute(query, params if params else None)
            results = cur.fetchall()
            LOGGER.debug(f"Retrieved {len(results)} skill level definitions from database.")

    return results


def getSkillLevelsBySkillId(skill_id):
    """
    Get all level definitions for a specific skill.

    Args:
        skill_id (str): The skill ID to query

    Returns:
        list: List of tuples with level definitions ordered by level
    """
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT
                    sld."Level",
                    sld."Description",
                    sld."Autonomy",
                    sld."Influence",
                    sld."Complexity",
                    sld."BusinessSkills",
                    sld."Knowledge",
                    sld."BehavioralIndicators",
                    sld."EvidenceExamples"
                FROM public."SkillLevelDefinitions" sld
                JOIN public."Skills" s ON sld."SkillId" = s."Id"
                WHERE NOT sld."IsDeleted"
                    AND NOT s."IsDeleted"
                    AND s."IsActive" = true
                    AND sld."SkillId" = %s
                ORDER BY sld."Level"
                """, (skill_id,))
            results = cur.fetchall()
            LOGGER.debug(f"Retrieved {len(results)} levels for skill ID: {skill_id}")

    return results


def getSkillDefinitionsByLevel(level):
    """
    Get all skill definitions for a specific proficiency level.

    Args:
        level (int): Proficiency level (1-7)

    Returns:
        list: List of tuples with skill definitions at the specified level
    """
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT
                    s."Name" as "SkillName",
                    s."Code" as "SkillCode",
                    sld."Description",
                    sld."Autonomy",
                    sld."Influence",
                    sld."Complexity",
                    sld."BusinessSkills",
                    sld."Knowledge"
                FROM public."SkillLevelDefinitions" sld
                JOIN public."Skills" s ON sld."SkillId" = s."Id"
                WHERE NOT sld."IsDeleted"
                    AND NOT s."IsDeleted"
                    AND s."IsActive" = true
                    AND sld."Level" = %s
                ORDER BY s."Name"
                """, (level,))
            results = cur.fetchall()
            LOGGER.debug(f"Retrieved {len(results)} skill definitions for level {level}")

    return results


def getSkillLevelCount():
    """
    Get total count of skill level definitions.

    Returns:
        int: Total count of non-deleted skill level definitions
    """
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT COUNT(*)
                FROM public."SkillLevelDefinitions"
                WHERE NOT "IsDeleted"
                """)
            count = cur.fetchone()[0]
            LOGGER.debug(f"Total skill level definitions: {count}")

    return count


def getDistinctSkillsWithLevels():
    """
    Get list of distinct skills that have level definitions.

    Returns:
        list: List of tuples (SkillId, SkillName, SkillCode, LevelCount)
    """
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT
                    s."Id",
                    s."Name",
                    s."Code",
                    COUNT(sld."Id") as "LevelCount"
                FROM public."Skills" s
                JOIN public."SkillLevelDefinitions" sld ON s."Id" = sld."SkillId"
                WHERE NOT s."IsDeleted"
                    AND NOT sld."IsDeleted"
                    AND s."IsActive" = true
                GROUP BY s."Id", s."Name", s."Code"
                ORDER BY s."Name"
                """)
            results = cur.fetchall()
            LOGGER.debug(f"Found {len(results)} distinct skills with level definitions")

    return results


def getSkillLevelDefinitionById(definition_id):
    """
    Get a specific skill level definition by its ID.

    Args:
        definition_id (str): The definition ID to query

    Returns:
        tuple: Single skill level definition record or None if not found
    """
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT
                    sld."Id",
                    sld."SkillId",
                    s."Name" as "SkillName",
                    sld."Level",
                    sld."Description",
                    sld."Autonomy",
                    sld."Influence",
                    sld."Complexity",
                    sld."BusinessSkills",
                    sld."Knowledge",
                    sld."BehavioralIndicators",
                    sld."EvidenceExamples"
                FROM public."SkillLevelDefinitions" sld
                JOIN public."Skills" s ON sld."SkillId" = s."Id"
                WHERE NOT sld."IsDeleted"
                    AND NOT s."IsDeleted"
                    AND s."IsActive" = true
                    AND sld."Id" = %s
                """, (definition_id,))
            result = cur.fetchone()
            if result:
                LOGGER.debug(f"Found skill level definition with ID: {definition_id}")
            else:
                LOGGER.warning(f"No skill level definition found with ID: {definition_id}")

    return result


# Example usage and testing
if __name__ == "__main__":
    import sys
    import io

    # Set UTF-8 encoding for console output
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

    print("=" * 80)
    print("Testing SkillLevelDefinitions Database Reader")
    print("=" * 80)

    # Test 1: Get total count
    print("\n1. Getting total count of skill level definitions...")
    total = getSkillLevelCount()
    print(f"   Total: {total}")

    # Test 2: Get distinct skills with levels
    print("\n2. Getting distinct skills with level definitions...")
    skills = getDistinctSkillsWithLevels()
    print(f"   Found {len(skills)} skills")
    if skills:
        print("   First 5 skills:")
        for skill in skills[:5]:
            print(f"   - {skill[1]} ({skill[2]}): {skill[3]} levels")

    # Test 3: Get all level definitions (limited)
    print("\n3. Getting first 5 skill level definitions...")
    definitions = getSkillLevelDefinitions()[:5]
    for defn in definitions:
        # Handle potential None values and Unicode characters
        desc = str(defn[5])[:50] if defn[5] else "No description"
        print(f"   - {defn[2]} (Level {defn[4]}): {desc}...")

    # Test 4: Get levels for first skill
    if skills:
        first_skill_id = skills[0][0]
        print(f"\n4. Getting all levels for skill: {skills[0][1]}")
        levels = getSkillLevelsBySkillId(first_skill_id)
        for level in levels:
            desc = str(level[1])[:60] if level[1] else "No description"
            print(f"   Level {level[0]}: {desc}...")

    # Test 5: Get all skills at level 3
    print("\n5. Getting all skills at Level 3...")
    level3_skills = getSkillDefinitionsByLevel(3)
    print(f"   Found {len(level3_skills)} skills at Level 3")
    if level3_skills:
        print("   First 3 skills:")
        for skill in level3_skills[:3]:
            print(f"   - {skill[0]} ({skill[1]})")

    print("\n" + "=" * 80)
    print("Testing completed!")
    print("=" * 80)
