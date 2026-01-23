# Question Generation Input Schema Guide

## Overview

Schema này định nghĩa cấu trúc input request cho API generate questions. File schema: `src/schemas/input_request_schema.json`

## Required Fields (Bắt buộc)

### 1. `question_type` (array)

Loại câu hỏi cần generate. **Có thể chọn 1 hoặc nhiều giá trị**.

**Allowed Values:**
- `"Multiple Choice"` - Trắc nghiệm 1 đáp án đúng
- `"Multiple Answer"` - Trắc nghiệm nhiều đáp án đúng
- `"True/False"` - Đúng/Sai
- `"Short Answer"` - Câu trả lời ngắn
- `"Long Answer"` - Câu trả lời dài (essay)
- `"Coding Challenge"` - Bài tập lập trình
- `"Scenario"` - Tình huống
- `"Situational Judgment"` - Đánh giá tình huống (SJT)
- `"Rating"` - Đánh giá thang điểm

**Example:**
```json
{
  "question_type": ["Multiple Choice", "Short Answer"]
}
```

**Database Mapping:**
Khi gửi đến database, các giá trị này sẽ được convert:
- `"Multiple Choice"` → `"MultipleChoice"`
- `"Multiple Answer"` → `"MultipleAnswer"`
- `"True/False"` → `"TrueFalse"`
- etc.

---

### 2. `language` (string)

Ngôn ngữ của câu hỏi và đáp án được generate.

**Allowed Values:**
- `"English"` - Tiếng Anh
- `"Vietnamese"` - Tiếng Việt

**Example:**
```json
{
  "language": "Vietnamese"
}
```

**Database Mapping:**
- `"English"` → `"en"`
- `"Vietnamese"` → `"vi"`

---

### 3. `number_of_questions` (integer)

Tổng số câu hỏi cần generate.

**Constraints:**
- Minimum: 1
- Maximum: 100

**Example:**
```json
{
  "number_of_questions": 20
}
```

---

## Optional Fields (Tùy chọn)

### 4. `skills` (array | null)

Skill(s) để generate câu hỏi. User select từ danh sách có sẵn trong DB.

**Nếu `null` hoặc empty**: Hệ thống sẽ **random** từ các skills có sẵn.

**Structure:**
```json
{
  "skills": [
    {
      "skill_id": "uuid",           // UUID from public.Skills
      "skill_name": "string",       // Tên skill
      "skill_code": "string"        // Mã skill (optional)
    }
  ]
}
```

**Example 1: Chọn specific skill**
```json
{
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000078",
      "skill_name": "Accessibility and inclusion",
      "skill_code": "ACIN"
    }
  ]
}
```

**Example 2: Random skill**
```json
{
  "skills": null
}
```

**Database Connection:**
- Lấy danh sách skills từ: `public.Skills` table
- Total skills available: **146**

---

### 5. `target_proficiency_level` (array | null)

Mức độ SFIA cần target. User select từ danh sách levels có sẵn cho skill đã chọn.

**Nếu `null` hoặc empty**: Hệ thống sẽ **random** từ các levels có sẵn cho skill.

**Allowed Values:** 1-7 (SFIA framework levels)

**SFIA Levels:**
- **Level 1** - Follow (Làm theo)
- **Level 2** - Assist (Hỗ trợ)
- **Level 3** - Apply (Áp dụng)
- **Level 4** - Enable (Kích hoạt)
- **Level 5** - Ensure/Advise (Đảm bảo/Tư vấn)
- **Level 6** - Initiate (Khởi xướng)
- **Level 7** - Set Strategy (Định chiến lược)

**Example 1: Specific levels**
```json
{
  "target_proficiency_level": [3, 4]
}
```

**Example 2: Random levels**
```json
{
  "target_proficiency_level": null
}
```

**Database Connection:**
- Lấy level definitions từ: `public.SkillLevelDefinitions` table
- Link qua `skill_id` từ field `skills` ở trên
- Query example:
  ```sql
  SELECT * FROM public."SkillLevelDefinitions"
  WHERE "SkillId" = <skill_id> AND "Level" IN (3, 4)
  ```

---

### 6. `difficulty` (string | null)

Mức độ khó của câu hỏi.

**Nếu `null`**: Hệ thống sẽ **random** difficulty.

**Allowed Values:**
- `"Easy"` - Dễ
- `"Medium"` - Trung bình
- `"Hard"` - Khó

**Example 1: Specific difficulty**
```json
{
  "difficulty": "Medium"
}
```

**Example 2: Random difficulty**
```json
{
  "difficulty": null
}
```

**Database Mapping:**
- `"Easy"` → `"easy"`
- `"Medium"` → `"medium"`
- `"Hard"` → `"hard"`

---

### 7. `additional_context` (string | null)

Thông tin thêm hoặc hướng dẫn để AI generate câu hỏi tốt hơn.

**Constraints:**
- Max length: 2000 characters

**Example:**
```json
{
  "additional_context": "Focus on WCAG 2.1 AA standards and practical implementation. Include real-world scenarios from e-commerce websites."
}
```

---

## Complete Examples

### Example 1: Full Request (All Fields Specified)

```json
{
  "question_type": ["Multiple Choice", "Short Answer", "Coding Challenge"],
  "language": "English",
  "number_of_questions": 15,
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000078",
      "skill_name": "Accessibility and inclusion",
      "skill_code": "ACIN"
    }
  ],
  "target_proficiency_level": [3, 4],
  "difficulty": "Medium",
  "additional_context": "Focus on WCAG 2.1 AA standards, keyboard navigation, and screen reader compatibility. Include code examples in HTML/CSS/JavaScript."
}
```

**AI sẽ generate:**
- 15 câu hỏi
- Loại: MCQ + Short Answer + Coding Challenge
- Ngôn ngữ: Tiếng Anh
- Skill: Accessibility and inclusion
- Levels: 3 (Apply) và 4 (Enable)
- Độ khó: Medium
- Context: WCAG 2.1 AA, keyboard, screen readers

---

### Example 2: Minimal Request (Random Skills/Levels/Difficulty)

```json
{
  "question_type": ["Multiple Choice", "True/False"],
  "language": "Vietnamese",
  "number_of_questions": 10,
  "skills": null,
  "target_proficiency_level": null,
  "difficulty": null,
  "additional_context": null
}
```

**AI sẽ generate:**
- 10 câu hỏi
- Loại: MCQ + True/False
- Ngôn ngữ: Tiếng Việt
- Skill: Random từ 146 skills có sẵn
- Levels: Random từ các levels có sẵn của skill được chọn
- Độ khó: Random (Easy/Medium/Hard)

---

### Example 3: Coding Challenge (Vietnamese)

```json
{
  "question_type": ["Coding Challenge"],
  "language": "Vietnamese",
  "number_of_questions": 5,
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "skill_name": "Programming",
      "skill_code": "PROG"
    }
  ],
  "target_proficiency_level": [2, 3],
  "difficulty": "Hard",
  "additional_context": "Sử dụng Python. Tập trung vào algorithms và data structures: sorting, searching, trees, graphs. Bao gồm test cases và giải thích độ phức tạp time/space."
}
```

---

### Example 4: Mixed Types with Multiple Skills

```json
{
  "question_type": ["Multiple Choice", "Scenario", "Situational Judgment"],
  "language": "English",
  "number_of_questions": 20,
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000010",
      "skill_name": "Project management",
      "skill_code": "PRMG"
    },
    {
      "skill_id": "30000000-0000-0000-0000-000000000011",
      "skill_name": "Stakeholder relationship management",
      "skill_code": "RLMT"
    }
  ],
  "target_proficiency_level": [4, 5],
  "difficulty": "Medium",
  "additional_context": "Focus on complex project scenarios with multiple stakeholders, conflict resolution, and strategic decision-making."
}
```

---

## Validation with Python

### Basic Validation

```python
from src.validators.request_validator import RequestValidator

validator = RequestValidator()

request_data = {
    "question_type": ["Multiple Choice"],
    "language": "English",
    "number_of_questions": 10
}

is_valid, error = validator.validate_request(request_data)
if is_valid:
    print("Request is valid!")
else:
    print(f"Validation error: {error}")
```

### Validate and Normalize

```python
from src.validators.request_validator import validate_and_normalize

request_data = {
    "question_type": ["Multiple Choice"],
    "language": "English",
    "number_of_questions": 10,
    "difficulty": "Medium"
}

is_valid, error, normalized = validate_and_normalize(request_data)

if is_valid:
    print(f"Normalized: {normalized}")
    # Output: question_type becomes ["MultipleChoice"]
    #         language becomes "en"
    #         difficulty becomes "medium"
else:
    print(f"Error: {error}")
```

### Create Sample Request

```python
from src.validators.request_validator import RequestValidator

validator = RequestValidator()

sample = validator.create_sample_request(
    question_types=["Multiple Choice", "Short Answer"],
    language="Vietnamese",
    num_questions=15,
    skill_id="30000000-0000-0000-0000-000000000078",
    skill_name="Accessibility and inclusion",
    levels=[3, 4],
    difficulty="Medium",
    context="Focus on WCAG"
)

print(sample)
```

---

## Database Integration Flow

### Step 1: Get Available Skills

```python
from db_skill_reader import getDistinctSkillsWithLevels

# Get all skills for dropdown
skills = getDistinctSkillsWithLevels()
# Returns: [(skill_id, skill_name, skill_code, level_count), ...]

# Format for frontend dropdown
skill_options = [
    {
        "skill_id": str(skill[0]),
        "skill_name": skill[1],
        "skill_code": skill[2],
        "available_levels": skill[3]
    }
    for skill in skills
]
```

### Step 2: Get Levels for Selected Skill

```python
from db_skill_reader import getSkillLevelsBySkillId

# When user selects a skill, get its available levels
skill_id = "30000000-0000-0000-0000-000000000078"
levels = getSkillLevelsBySkillId(skill_id)

# Format for frontend dropdown
level_options = [
    {
        "level": level[0],
        "description": level[1][:100] + "..."
    }
    for level in levels
]
```

### Step 3: Build Request

```python
# User selected values
request = {
    "question_type": ["Multiple Choice", "Short Answer"],
    "language": "Vietnamese",
    "number_of_questions": 10,
    "skills": [{
        "skill_id": selected_skill_id,
        "skill_name": selected_skill_name,
        "skill_code": selected_skill_code
    }],
    "target_proficiency_level": [3, 4],
    "difficulty": "Medium",
    "additional_context": user_input_context
}
```

### Step 4: Validate and Send to AI

```python
from src.validators.request_validator import validate_and_normalize

# Validate
is_valid, error, normalized = validate_and_normalize(request)

if not is_valid:
    return {"error": error}

# Get skill data from DB
from src.custom import getSkillData

skill_data = getSkillData(normalized["skills"][0]["skill_id"])

# Send to AI generator
# ... (AI generation logic)
```

---

## Error Handling

### Common Validation Errors

1. **Missing required field**
   ```json
   {
     "question_type": ["Multiple Choice"],
     "language": "English"
     // Missing: number_of_questions
   }
   ```
   Error: `'number_of_questions' is a required property`

2. **Invalid question type**
   ```json
   {
     "question_type": ["Invalid Type"],
     ...
   }
   ```
   Error: `'Invalid Type' is not one of ['Multiple Choice', 'Multiple Answer', ...]`

3. **Invalid number range**
   ```json
   {
     "number_of_questions": 0  // Must be >= 1
   }
   ```
   Error: `0 is less than the minimum of 1`

4. **Invalid proficiency level**
   ```json
   {
     "target_proficiency_level": [0, 8]  // Must be 1-7
   }
   ```
   Error: `0 is less than the minimum of 1`

---

## Testing

Run validator tests:

```bash
# Windows (with venv)
.venv\Scripts\python.exe ai-gen\src\validators\request_validator.py

# Linux/Mac (with venv)
.venv/bin/python ai-gen/src/validators/request_validator.py
```

---

## Summary

| Field | Type | Required | Default | Values |
|-------|------|----------|---------|--------|
| `question_type` | array | ✅ Yes | - | 9 types |
| `language` | string | ✅ Yes | - | English/Vietnamese |
| `number_of_questions` | integer | ✅ Yes | - | 1-100 |
| `skills` | array\|null | ❌ No | null | From DB (146 skills) |
| `target_proficiency_level` | array\|null | ❌ No | null | 1-7 (SFIA) |
| `difficulty` | string\|null | ❌ No | null | Easy/Medium/Hard |
| `additional_context` | string\|null | ❌ No | null | Max 2000 chars |

**Total Database Records:**
- Skills: 146
- Skill Level Definitions: 589
- Average levels per skill: ~4 levels
