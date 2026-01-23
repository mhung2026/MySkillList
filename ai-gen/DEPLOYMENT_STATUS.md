# API Deployment Status Report

**Date**: 2026-01-23
**Status**: ⚠️ **CHƯA DEPLOY SCHEMA MỚI**

---

## 📊 Current Status

### ❌ Schema Mới CHƯA được Deploy

| Component | Old Version | New Version | Status |
|-----------|-------------|-------------|--------|
| **Input Schema** | `skill_data` format | `input_request_schema.json` | ❌ Not deployed |
| **Output Schema** | `output_question_schema.json` | `output_question_schema_v2.json` | ❌ Not deployed |
| **Request Validator** | `input_validator.py` | `request_validator.py` | ✅ Created, not used |
| **DB Reader** | N/A | `db_skill_reader.py` | ✅ Created, not used |

---

## 📁 Files Analysis

### 1. Main API File: `main.py`

**Current Imports** (Lines 10-11):
```python
from src.validators.input_validator import validate_input_skill
from src.validators.output_validator import validate_output_questions
```
❌ **Still using OLD validators**

**Current Request Format** (Lines 59-63):
```python
class GenerateRequest(BaseModel):
    skill_data: SkillInput  # OLD format
    num_questions: int
    language: str
    min_per_level: int
```
❌ **Still using OLD request format**

**Should be** (NEW format):
```python
from src.validators.request_validator import validate_and_normalize
from db_skill_reader import getSkillLevelsBySkillId

class GenerateRequest(BaseModel):
    question_type: List[str]           # NEW
    language: str                      # "English" or "Vietnamese"
    number_of_questions: int           # NEW field name
    skills: Optional[List[Dict]]       # NEW
    target_proficiency_level: Optional[List[int]]  # NEW
    difficulty: Optional[str]          # NEW
    additional_context: Optional[str]  # NEW
```

---

### 2. Routes File: `src/api/routes.py`

**Current Endpoint** (Lines 15-21):
```python
@router.post("/generate-questions", response_model=List[Dict])
async def generate_questions_endpoint(skill_input: SkillInput):
    questions = generate_questions(
        skill_input.skill,      # OLD format
        num_questions=10,
        language="en",
        min_per_level=2
    )
```
❌ **Still using OLD input format**

---

### 3. Database Functions

**Status**:
- ✅ `db_skill_reader.py` created with 6 functions
- ❌ **NOT imported** in main.py
- ❌ **NOT used** in any endpoint

**Available Functions**:
```python
getSkillLevelDefinitions()
getSkillLevelsBySkillId()
getSkillDefinitionsByLevel()
getSkillLevelCount()
getDistinctSkillsWithLevels()
getSkillLevelDefinitionById()
```

---

### 4. Validators

**Files**:
```
✅ src/validators/request_validator.py   (9 KB) - Created, not used
✅ src/validators/input_validator.py     (546 B) - OLD, currently in use
✅ src/validators/output_validator.py    (591 B) - OLD, currently in use
```

---

## 🚫 API Server Status

**Checked Port**: 8002
**Status**: ❌ **NOT RUNNING**

```bash
netstat -ano | grep ":8002"
# No output - API not running
```

---

## ✅ What's Ready (Not Deployed)

### Input Schema V2
- ✅ Schema file: `src/schemas/input_request_schema.json`
- ✅ Validator: `src/validators/request_validator.py`
- ✅ Documentation: `INPUT_SCHEMA_GUIDE.md`

### Output Schema V2
- ✅ Schema file: `src/schemas/output_question_schema_v2.json`
- ✅ Documentation: `OUTPUT_SCHEMA_V2_GUIDE.md`
- ✅ Examples: 9 complete examples

### Database Integration
- ✅ DB Reader: `db_skill_reader.py`
- ✅ Connection tested: Working (589 definitions, 146 skills)
- ✅ Documentation: `DB_CONNECTION_GUIDE.md`

---

## ❌ What's Missing

### 1. Update main.py
- [ ] Import new validators
- [ ] Import db_skill_reader
- [ ] Create new request model (input_request_schema)
- [ ] Update /generate-questions endpoint
- [ ] Add new endpoints for skills/levels

### 2. Update Generator
- [ ] Modify question_generator.py to accept new input format
- [ ] Output questions in V2 format
- [ ] Support all 9 question types

### 3. Add New Endpoints
- [ ] `GET /skills` - List all skills from DB
- [ ] `GET /skills/{skill_id}/levels` - Get levels for a skill
- [ ] `POST /generate-questions-v2` - New endpoint with V2 schema

### 4. Testing
- [ ] Test new endpoints
- [ ] Validate V2 input/output
- [ ] Integration testing

---

## 📋 Deployment Checklist

### Phase 1: Update API Endpoints ⏳ TODO

1. **Create new endpoint file**: `src/api/routes_v2.py`
   ```python
   from src.validators.request_validator import validate_and_normalize
   from db_skill_reader import getDistinctSkillsWithLevels, getSkillLevelsBySkillId

   @router.get("/api/v2/skills")
   async def get_all_skills():
       """Get list of all skills"""

   @router.get("/api/v2/skills/{skill_id}/levels")
   async def get_skill_levels(skill_id: str):
       """Get levels for a specific skill"""

   @router.post("/api/v2/generate-questions")
   async def generate_questions_v2(request: GenerateRequestV2):
       """Generate questions with V2 schema"""
   ```

2. **Update main.py**
   - Import routes_v2
   - Mount router: `app.include_router(routes_v2.router)`

3. **Create Pydantic models for V2**
   ```python
   class SkillInfo(BaseModel):
       skill_id: str
       skill_name: str
       skill_code: Optional[str]

   class GenerateRequestV2(BaseModel):
       question_type: List[str]
       language: str
       number_of_questions: int
       skills: Optional[List[SkillInfo]]
       target_proficiency_level: Optional[List[int]]
       difficulty: Optional[str]
       additional_context: Optional[str]
   ```

### Phase 2: Update Generator ⏳ TODO

1. **Modify `question_generator.py`**
   - Accept new input format
   - Generate questions in V2 format
   - Support all 9 question types

2. **Create output formatter**
   - Transform LLM output to V2 schema
   - Add metadata (timestamp, model, skill info)

### Phase 3: Testing ⏳ TODO

1. **Unit tests**
   - Test validators
   - Test DB functions
   - Test new endpoints

2. **Integration tests**
   - End-to-end flow
   - V2 schema validation

---

## 🚀 Quick Deploy Guide

### Step 1: Create New Routes File

```python
# src/api/routes_v2.py
from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
from datetime import datetime

from ..validators.request_validator import validate_and_normalize
from db_skill_reader import (
    getDistinctSkillsWithLevels,
    getSkillLevelsBySkillId,
    getSkillLevelCount
)

router = APIRouter(prefix="/api/v2", tags=["API V2"])

class SkillInfo(BaseModel):
    skill_id: str
    skill_name: str
    skill_code: Optional[str] = None

class GenerateRequestV2(BaseModel):
    question_type: List[str]
    language: str
    number_of_questions: int
    skills: Optional[List[SkillInfo]] = None
    target_proficiency_level: Optional[List[int]] = None
    difficulty: Optional[str] = None
    additional_context: Optional[str] = None

@router.get("/skills")
async def get_all_skills():
    """Get all available skills from database"""
    try:
        skills = getDistinctSkillsWithLevels()
        return {
            "skills": [
                {
                    "skill_id": str(s[0]),
                    "skill_name": s[1],
                    "skill_code": s[2],
                    "level_count": s[3]
                }
                for s in skills
            ],
            "total": len(skills)
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/skills/{skill_id}/levels")
async def get_skill_levels(skill_id: str):
    """Get proficiency levels for a specific skill"""
    try:
        levels = getSkillLevelsBySkillId(skill_id)
        return {
            "skill_id": skill_id,
            "levels": [
                {
                    "level": l[0],
                    "description": l[1],
                    "autonomy": l[2],
                    "influence": l[3],
                    "complexity": l[4]
                }
                for l in levels
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/generate-questions")
async def generate_questions_v2(request: GenerateRequestV2):
    """Generate questions using V2 schema"""
    try:
        # 1. Validate and normalize
        is_valid, error, normalized = validate_and_normalize(request.dict())
        if not is_valid:
            raise HTTPException(status_code=422, detail=error)

        # 2. Fetch skill data from DB if needed
        # ... (to be implemented)

        # 3. Generate questions
        # ... (to be implemented)

        return {
            "success": True,
            "message": "V2 endpoint - implementation in progress"
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
```

### Step 2: Update main.py

```python
# Add to imports
from src.api import routes_v2

# Add after existing middleware
app.include_router(routes_v2.router)
```

### Step 3: Run API

```bash
cd ai-gen
.venv\Scripts\python.exe main.py
```

### Step 4: Test Endpoints

```bash
# Get all skills
curl http://localhost:8002/api/v2/skills

# Get levels for a skill
curl http://localhost:8002/api/v2/skills/{skill_id}/levels

# Generate questions
curl -X POST http://localhost:8002/api/v2/generate-questions \
  -H "Content-Type: application/json" \
  -d '{
    "question_type": ["Multiple Choice"],
    "language": "English",
    "number_of_questions": 10
  }'
```

---

## 📝 Summary

| Item | Status | Action Needed |
|------|--------|---------------|
| **Schema Design** | ✅ Complete | None |
| **Documentation** | ✅ Complete | None |
| **Validators** | ✅ Created | Import & use in API |
| **DB Functions** | ✅ Created | Import & use in API |
| **API Endpoints** | ❌ Not updated | Create V2 routes |
| **Generator** | ❌ Not updated | Update to V2 format |
| **Testing** | ❌ Not done | Write tests |
| **Deployment** | ❌ Not deployed | Follow guide above |

---

## ⚡ Next Immediate Steps

1. **Create `routes_v2.py`** with code above
2. **Update `main.py`** to include new router
3. **Test basic endpoints** (skills, levels)
4. **Implement question generation** with V2 format
5. **Full integration testing**

---

**Last Updated**: 2026-01-23
**Updated By**: AI Assistant
**Status**: Ready for deployment
