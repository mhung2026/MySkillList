# Schema Comparison: Old vs New

## Overview

Document này so sánh schema cũ và schema mới cho hệ thống generate questions.

---

## Schema Architecture

### OLD Architecture
```
input_skill_schema.json (skill data)
         ↓
    AI Generator
         ↓
output_question_schema.json (questions)
```

### NEW Architecture
```
input_request_schema.json (user request)
         ↓
    Fetch from DB (using db_skill_reader.py)
         ↓
input_skill_schema.json (skill data - still used internally)
         ↓
    AI Generator
         ↓
output_question_schema.json (questions - unchanged)
```

---

## Input Schema Changes

### OLD: `input_skill_schema.json`

**Purpose**: Directly provide skill data to AI

```json
{
  "skill_name": "Python Programming",
  "skill_id": "uuid",
  "levels": [
    {
      "level": 3,
      "description": "...",
      "autonomy": "...",
      "influence": "...",
      "complexity": "...",
      "business_skills": "...",
      "knowledge": "...",
      "behavioral_indicators": [...],
      "evidence_examples": [...]
    }
  ]
}
```

**Issues:**
- ❌ User phải manually tạo skill data structure
- ❌ Không có cách specify question types
- ❌ Không có cách specify language preference
- ❌ Không có cách specify số lượng questions
- ❌ Không có cách specify difficulty level
- ❌ Không có cách random skills/levels

---

### NEW: `input_request_schema.json`

**Purpose**: User-friendly request format with automatic DB lookup

```json
{
  "question_type": ["Multiple Choice", "Short Answer"],
  "language": "Vietnamese",
  "number_of_questions": 10,
  "skills": [
    {
      "skill_id": "uuid",
      "skill_name": "name"
    }
  ],
  "target_proficiency_level": [3, 4],
  "difficulty": "Medium",
  "additional_context": "Focus on..."
}
```

**Benefits:**
- ✅ User chỉ cần chọn từ dropdowns (không cần biết DB structure)
- ✅ Support multiple question types
- ✅ Support language selection (EN/VI)
- ✅ Specify exact number of questions needed
- ✅ Support difficulty levels
- ✅ Auto-fetch skill data from DB
- ✅ Support random skills/levels/difficulty (null values)
- ✅ Additional context for better question quality

**Note:** `input_skill_schema.json` vẫn được sử dụng **internally** sau khi fetch data từ DB.

---

## Field-by-Field Comparison

| Feature | OLD Schema | NEW Schema |
|---------|------------|------------|
| **User specifies question type** | ❌ No | ✅ Yes (9 types) |
| **User specifies language** | ❌ No | ✅ Yes (EN/VI) |
| **User specifies quantity** | ❌ No | ✅ Yes (1-100) |
| **User specifies difficulty** | ❌ No | ✅ Yes (Easy/Medium/Hard) |
| **Random skill selection** | ❌ No | ✅ Yes (null = random) |
| **Random level selection** | ❌ No | ✅ Yes (null = random) |
| **Additional context** | ❌ No | ✅ Yes (2000 chars) |
| **Multiple skills** | ❌ No | ✅ Yes (array) |
| **Skill data source** | Manual input | ✅ Auto from DB |
| **Validation** | Basic | ✅ Comprehensive |
| **DB integration** | ❌ No | ✅ Yes |

---

## Question Type Mapping

### NEW Schema → Database Enum

| User-Friendly Name | Database Enum |
|--------------------|---------------|
| Multiple Choice | MultipleChoice |
| Multiple Answer | MultipleAnswer |
| True/False | TrueFalse |
| Short Answer | ShortAnswer |
| Long Answer | LongAnswer |
| Coding Challenge | CodingChallenge |
| Scenario | Scenario |
| Situational Judgment | SituationalJudgment |
| Rating | Rating |

**Handled by**: `RequestValidator.normalize_request()`

---

## Language Mapping

| User Input | Internal Code |
|------------|---------------|
| English | en |
| Vietnamese | vi |

---

## Difficulty Mapping

| User Input | Internal Code |
|------------|---------------|
| Easy | easy |
| Medium | medium |
| Hard | hard |

---

## Output Schema (Unchanged)

`output_question_schema.json` **không thay đổi**. Vẫn support:

- **Question Types**: mcq, true_false, short_answer, essay, coding
- **Fields**: id, type, stem, language, difficulty, choices, answer, etc.
- **Conditional validation** based on question type

**Note**: Output schema có 5 types, nhưng input request có 9 types. Mapping sẽ như sau:

| Input Request Type | Output Question Type |
|-------------------|---------------------|
| Multiple Choice | mcq |
| Multiple Answer | mcq (with multiple correct) |
| True/False | true_false |
| Short Answer | short_answer |
| Long Answer | essay |
| Coding Challenge | coding |
| Scenario | essay (với context field) |
| Situational Judgment | mcq (với scenario stem) |
| Rating | short_answer (với rating metadata) |

---

## Migration Guide

### For API Developers

**OLD Code:**
```python
# Manual skill data creation
skill_data = {
    "skill_name": "Python",
    "levels": [...]  # Manually created
}

# No control over question types, language, etc.
questions = generate_questions(skill_data)
```

**NEW Code:**
```python
from src.validators.request_validator import validate_and_normalize
from db_skill_reader import getSkillLevelsBySkillId
from src.custom import getSkillData

# 1. User request
request = {
    "question_type": ["Multiple Choice", "Coding Challenge"],
    "language": "Vietnamese",
    "number_of_questions": 10,
    "skills": [{"skill_id": "uuid", "skill_name": "Python"}],
    "target_proficiency_level": [3, 4],
    "difficulty": "Medium",
    "additional_context": "Focus on OOP"
}

# 2. Validate and normalize
is_valid, error, normalized = validate_and_normalize(request)

# 3. Fetch skill data from DB
skill_data = getSkillData(normalized["skills"][0]["skill_id"])

# 4. Generate with full control
questions = generate_questions(
    skill_data=skill_data,
    question_types=normalized["question_type"],
    language=normalized["language"],
    num_questions=normalized["number_of_questions"],
    difficulty=normalized["difficulty"],
    context=normalized["additional_context"]
)
```

---

### For Frontend Developers

**API Endpoint Example:**

```javascript
// POST /api/generate-questions

const request = {
  question_type: ["Multiple Choice", "Short Answer"],
  language: "Vietnamese",
  number_of_questions: 10,
  skills: [
    {
      skill_id: selectedSkillId,  // From dropdown
      skill_name: selectedSkillName,
      skill_code: selectedSkillCode
    }
  ],
  target_proficiency_level: [3, 4],  // From checkbox
  difficulty: "Medium",  // From dropdown
  additional_context: userInput  // From textarea
};

const response = await fetch('/api/generate-questions', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(request)
});

const questions = await response.json();
```

**Dropdown Data Sources:**

```javascript
// 1. Get all skills
const skillsResponse = await fetch('/api/skills');
const skills = await skillsResponse.json();
// Returns: [{ skill_id, skill_name, skill_code }, ...]

// 2. Get levels for selected skill
const levelsResponse = await fetch(`/api/skills/${skillId}/levels`);
const levels = await levelsResponse.json();
// Returns: [{ level, description, autonomy, ... }, ...]
```

---

## File Structure

### NEW Files Created

```
ai-gen/
├── src/
│   ├── schemas/
│   │   ├── input_skill_schema.json         # Still used internally
│   │   ├── input_request_schema.json       # ✨ NEW - User request
│   │   └── output_question_schema.json     # Unchanged
│   └── validators/
│       └── request_validator.py            # ✨ NEW - Request validation
├── db_skill_reader.py                      # ✨ NEW - DB integration
├── INPUT_SCHEMA_GUIDE.md                   # ✨ NEW - Documentation
├── SCHEMA_COMPARISON.md                    # ✨ NEW - This file
└── DB_CONNECTION_GUIDE.md                  # ✨ NEW - DB guide
```

---

## Benefits Summary

### For Users
- ✅ Easier to use (dropdown selections instead of JSON)
- ✅ More control (question types, language, difficulty)
- ✅ Random options (don't have to specify everything)
- ✅ Context field for better questions

### For Developers
- ✅ Automatic DB integration
- ✅ Validation built-in
- ✅ Normalization handled
- ✅ Type-safe with schema validation

### For System
- ✅ Single source of truth (DB)
- ✅ Consistent data format
- ✅ Better error handling
- ✅ Scalable architecture

---

## Testing

### Test Request Validation
```bash
.venv\Scripts\python.exe ai-gen\src\validators\request_validator.py
```

### Test DB Connection
```bash
.venv\Scripts\python.exe ai-gen\db_skill_reader.py
```

---

## Backward Compatibility

**Question**: Can we still use old `input_skill_schema.json`?

**Answer**: YES! Old schema vẫn được sử dụng internally.

**Flow**:
```
New Request → Fetch DB → Old Skill Schema → AI → Output
```

Internal code vẫn expect skill data theo format cũ. Chỉ thêm layer mới ở đầu để:
1. Validate user request
2. Fetch từ DB
3. Transform thành skill schema format
4. Pass to existing AI generator

---

## Next Steps

1. ✅ **Completed**:
   - Created `input_request_schema.json`
   - Created `request_validator.py`
   - Created `db_skill_reader.py`
   - Created documentation

2. **TODO**:
   - [ ] Update API endpoints to use new request schema
   - [ ] Create frontend form components
   - [ ] Integrate with AI generator
   - [ ] Add caching for DB queries
   - [ ] Add request logging
   - [ ] Create API documentation (Swagger)

---

## Questions?

- Request schema: `INPUT_SCHEMA_GUIDE.md`
- DB connection: `DB_CONNECTION_GUIDE.md`
- Validation examples: Run `request_validator.py`
- DB examples: Run `db_skill_reader.py`
