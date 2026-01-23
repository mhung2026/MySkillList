# Schema Update Summary

## 📋 Tóm Tắt Thay Đổi

Đã tạo **Input Request Schema** mới để user dễ dàng generate questions với đầy đủ options.

---

## 📥 Input Schema Mới: `input_request_schema.json`

### Required Fields (Bắt buộc)

1. **`question_type`** (array) - Loại câu hỏi
   - Multiple Choice, Multiple Answer, True/False, Short Answer, Long Answer
   - Coding Challenge, Scenario, Situational Judgment, Rating

2. **`language`** (string) - Ngôn ngữ
   - "English" hoặc "Vietnamese"

3. **`number_of_questions`** (integer) - Số câu hỏi
   - Từ 1 đến 100

### Optional Fields (Tùy chọn - null = random)

4. **`skills`** (array | null) - Chọn skill từ DB
   - 146 skills có sẵn
   - Null = random skill

5. **`target_proficiency_level`** (array | null) - Chọn SFIA level
   - Levels 1-7
   - Null = random level

6. **`difficulty`** (string | null) - Độ khó
   - "Easy", "Medium", "Hard"
   - Null = random

7. **`additional_context`** (string | null) - Ghi chú thêm
   - Max 2000 ký tự

---

## 📄 Files Mới Tạo

### 1. Schemas
- ✅ `src/schemas/input_request_schema.json` - Request schema

### 2. Validators
- ✅ `src/validators/request_validator.py` - Validation & normalization

### 3. Database
- ✅ `db_skill_reader.py` - 6 functions để đọc DB
- ✅ `DB_CONNECTION_GUIDE.md` - Hướng dẫn connect DB

### 4. Documentation
- ✅ `INPUT_SCHEMA_GUIDE.md` - Chi tiết về input schema (60+ examples)
- ✅ `SCHEMA_COMPARISON.md` - So sánh schema cũ/mới
- ✅ `SCHEMA_UPDATE_SUMMARY.md` - Document này

---

## 🎯 Ví Dụ Request

### Example 1: Full Request
```json
{
  "question_type": ["Multiple Choice", "Short Answer"],
  "language": "English",
  "number_of_questions": 10,
  "skills": [{
    "skill_id": "30000000-0000-0000-0000-000000000078",
    "skill_name": "Accessibility and inclusion",
    "skill_code": "ACIN"
  }],
  "target_proficiency_level": [3, 4],
  "difficulty": "Medium",
  "additional_context": "Focus on WCAG 2.1 AA standards"
}
```

### Example 2: Random (Minimal)
```json
{
  "question_type": ["Multiple Choice"],
  "language": "Vietnamese",
  "number_of_questions": 10,
  "skills": null,
  "target_proficiency_level": null,
  "difficulty": null,
  "additional_context": null
}
```

---

## ✅ Testing

### Test Validator
```bash
.venv\Scripts\python.exe ai-gen\src\validators\request_validator.py
```

**Results:**
- ✅ Valid requests accepted
- ✅ Invalid requests rejected with clear errors
- ✅ Normalization working (English→en, Medium→medium)
- ✅ SFIA level names mapped correctly

### Test DB Reader
```bash
.venv\Scripts\python.exe ai-gen\db_skill_reader.py
```

**Results:**
- ✅ Connected to PostgreSQL successfully
- ✅ Total 589 skill level definitions
- ✅ 146 distinct skills
- ✅ Unicode/Vietnamese text handled correctly

---

## 🔄 Data Flow

```
1. User Input (Frontend)
   ↓
2. POST /api/generate-questions
   ↓
3. Validate with request_validator.py
   ↓
4. Fetch skill data from DB (db_skill_reader.py)
   ↓
5. Transform to skill_schema format
   ↓
6. Send to AI Generator
   ↓
7. Return questions (output_question_schema.json)
```

---

## 📊 Database Stats

- **Total Skills**: 146
- **Total Skill Level Definitions**: 589
- **SFIA Levels**: 1-7
- **Average Levels per Skill**: ~4 levels

**Top Skills by Level Count:**
- Accessibility and inclusion (ACIN): 5 levels
- Analytical classification and coding (ANCC): 4 levels
- Animation development (ADEV): 5 levels

---

## 🔧 Python Usage

### Validate Request
```python
from src.validators.request_validator import validate_and_normalize

request = {...}
is_valid, error, normalized = validate_and_normalize(request)
```

### Get Skills from DB
```python
from db_skill_reader import getDistinctSkillsWithLevels

skills = getDistinctSkillsWithLevels()
# Returns: [(skill_id, skill_name, skill_code, level_count), ...]
```

### Get Levels for Skill
```python
from db_skill_reader import getSkillLevelsBySkillId

levels = getSkillLevelsBySkillId("skill_uuid")
# Returns: [(level, description, autonomy, ...), ...]
```

---

## 📚 Documentation Links

| Document | Purpose |
|----------|---------|
| `INPUT_SCHEMA_GUIDE.md` | Chi tiết về request schema (60+ ví dụ) |
| `DB_CONNECTION_GUIDE.md` | Hướng dẫn kết nối DB và query functions |
| `SCHEMA_COMPARISON.md` | So sánh schema cũ vs mới |
| `SCHEMA_UPDATE_SUMMARY.md` | Summary này |

---

## ⚡ Quick Start

### 1. Validate một request
```python
from src.validators.request_validator import RequestValidator

validator = RequestValidator()
is_valid, error = validator.validate_request(your_request)
```

### 2. Lấy danh sách skills
```python
from db_skill_reader import getDistinctSkillsWithLevels

skills = getDistinctSkillsWithLevels()
```

### 3. Tạo sample request
```python
from src.validators.request_validator import RequestValidator

validator = RequestValidator()
sample = validator.create_sample_request(
    question_types=["Multiple Choice"],
    language="Vietnamese",
    num_questions=10
)
```

---

## 🎨 Frontend Integration

### Get Skills for Dropdown
```javascript
const response = await fetch('/api/skills');
const skills = await response.json();
// Use skills for <Select> options
```

### Submit Request
```javascript
const request = {
  question_type: selectedTypes,  // From multi-select
  language: selectedLanguage,    // From radio buttons
  number_of_questions: count,    // From input number
  skills: selectedSkills,        // From select dropdown
  target_proficiency_level: selectedLevels,  // From checkboxes
  difficulty: selectedDifficulty,  // From select
  additional_context: contextText  // From textarea
};

const response = await fetch('/api/generate-questions', {
  method: 'POST',
  body: JSON.stringify(request)
});
```

---

## ✨ Key Features

### For Users
- ✅ Easy dropdown selections
- ✅ Random options (don't specify = random)
- ✅ Multiple question types in one request
- ✅ Language selection
- ✅ Custom context for better questions

### For Developers
- ✅ Automatic validation
- ✅ Automatic DB lookup
- ✅ Normalized output
- ✅ Clear error messages
- ✅ Type-safe schemas

---

## 🚀 Next Steps

1. **Backend**: Update API endpoints to use new schema
2. **Frontend**: Create form with all fields
3. **Integration**: Connect form → API → DB → AI
4. **Testing**: End-to-end testing
5. **Documentation**: API docs (Swagger)

---

## ❓ Questions

See detailed docs:
- Schema details: `INPUT_SCHEMA_GUIDE.md`
- DB functions: `DB_CONNECTION_GUIDE.md`
- Schema comparison: `SCHEMA_COMPARISON.md`
