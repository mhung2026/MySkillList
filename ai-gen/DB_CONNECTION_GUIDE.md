# Database Connection Guide

## Overview

The `db_skill_reader.py` module provides easy access to the `SkillLevelDefinitions` table in PostgreSQL.

## Connection Configuration

Connection details are stored in `src/custom.py`:

```python
DB_CONFIG = {
    "host": "192.168.0.21",
    "database": "MySkillList_NGE_DEV",
    "user": "postgres",
    "password": "@ll1@nceP@ss2o21",
    "port": 5432
}
```

## Database Statistics

- **Total Skill Level Definitions**: 589
- **Distinct Skills**: 146
- **Proficiency Levels**: 1-7 (SFIA framework)

## Available Functions

### 1. Get All Skill Level Definitions

```python
from db_skill_reader import getSkillLevelDefinitions

# Get all definitions
all_definitions = getSkillLevelDefinitions()

# Filter by skill ID
definitions = getSkillLevelDefinitions(skill_id="30000000-0000-0000-0000-000000000078")

# Filter by level
level3_definitions = getSkillLevelDefinitions(level=3)

# Filter by both
specific_def = getSkillLevelDefinitions(skill_id="...", level=3)
```

**Returns**: List of tuples with columns:
- Id, SkillId, SkillName, SkillCode, Level, Description, Autonomy, Influence, Complexity, BusinessSkills, Knowledge, BehavioralIndicators, EvidenceExamples, IsDeleted, CreatedAt, UpdatedAt

### 2. Get Levels for Specific Skill

```python
from db_skill_reader import getSkillLevelsBySkillId

# Get all levels for a skill
levels = getSkillLevelsBySkillId("30000000-0000-0000-0000-000000000078")
```

**Returns**: List of tuples ordered by level (1-7)

### 3. Get Skills at Specific Level

```python
from db_skill_reader import getSkillDefinitionsByLevel

# Get all skills at level 3
level3_skills = getSkillDefinitionsByLevel(3)
```

**Returns**: 110 skills at level 3

### 4. Get Total Count

```python
from db_skill_reader import getSkillLevelCount

total = getSkillLevelCount()
# Returns: 589
```

### 5. Get Distinct Skills with Level Counts

```python
from db_skill_reader import getDistinctSkillsWithLevels

skills = getDistinctSkillsWithLevels()
# Returns: [(SkillId, SkillName, SkillCode, LevelCount), ...]
```

Example output:
- Accessibility and inclusion (ACIN): 5 levels
- Analytical classification and coding (ANCC): 4 levels
- Animation development (ADEV): 5 levels

### 6. Get Specific Definition by ID

```python
from db_skill_reader import getSkillLevelDefinitionById

definition = getSkillLevelDefinitionById("definition_id_here")
```

## Complete Example

```python
from src.custom import LOGGER
from db_skill_reader import (
    getSkillLevelDefinitions,
    getSkillLevelsBySkillId,
    getDistinctSkillsWithLevels
)

# 1. Get all available skills
skills = getDistinctSkillsWithLevels()
print(f"Found {len(skills)} skills")

# 2. Pick first skill
if skills:
    skill_id = skills[0][0]
    skill_name = skills[0][1]

    # 3. Get all levels for that skill
    levels = getSkillLevelsBySkillId(skill_id)

    print(f"\nSkill: {skill_name}")
    for level_data in levels:
        level_num = level_data[0]
        description = level_data[1]
        autonomy = level_data[2]

        print(f"  Level {level_num}:")
        print(f"    Description: {description}")
        print(f"    Autonomy: {autonomy}")
```

## Running the Test

```bash
# Windows (with venv)
.venv\Scripts\python.exe ai-gen\db_skill_reader.py

# Linux/Mac (with venv)
.venv/bin/python ai-gen/db_skill_reader.py
```

## Database Schema

The `SkillLevelDefinitions` table joins with `Skills` table:

```sql
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
    sld."BehavioralIndicators",  -- JSON array
    sld."EvidenceExamples"        -- JSON array
FROM public."SkillLevelDefinitions" sld
JOIN public."Skills" s ON sld."SkillId" = s."Id"
WHERE NOT sld."IsDeleted" AND NOT s."IsDeleted"
```

## Connection Pattern

All functions use the context manager pattern from `src.custom`:

```python
from src.custom import getConn, LOGGER

def your_function():
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute("YOUR SQL HERE", (params,))
            results = cur.fetchall()
            LOGGER.debug("Success message")
    return results
```

## Notes

- Connection uses pooling (max 1000 connections)
- All queries filter out soft-deleted records (`IsDeleted = false`)
- Unicode/Vietnamese text is properly handled
- Logging is enabled for all database operations
