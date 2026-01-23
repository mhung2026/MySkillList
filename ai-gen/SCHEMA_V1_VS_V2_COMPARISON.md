# Output Schema V1 vs V2 Comparison

## Quick Summary

| Aspect | V1 (Old) | V2 (New) |
|--------|----------|----------|
| **Purpose** | Generic question format | Backend integration ready |
| **DB Mapping** | ❌ Indirect | ✅ Direct (CreateQuestionDto) |
| **Question Types** | 5 | ✅ 9 (full support) |
| **Auto-grading** | ⚠️ Partial | ✅ Complete |
| **Complexity** | ❌ High (45 fields) | ✅ Low (15 core fields) |
| **File Size** | 4.5 KB | 25 KB (với examples) |

---

## Detailed Comparison

### 1. Question Types Support

#### V1 Schema
```json
"type": {
  "enum": ["mcq", "true_false", "short_answer", "essay", "coding"]
}
```
**Supported**: 5 types
**Missing**: MultipleAnswer, Scenario, SJT, Rating

#### V2 Schema
```json
"type": {
  "enum": [
    "MultipleChoice", "MultipleAnswer", "TrueFalse",
    "ShortAnswer", "LongAnswer", "CodingChallenge",
    "Scenario", "SituationalJudgment", "Rating"
  ]
}
```
**Supported**: 9 types (100% coverage)
**Mapping**: Direct to QuestionType enum

---

### 2. Field Structure

#### V1 Required Fields
```json
{
  "id": "string",
  "type": "mcq",
  "stem": "question text",
  "language": "en",
  "difficulty": "medium"
}
```
**Issues**:
- `id`: String pattern (không phải UUID)
- `stem`: Tên field không match DB (`content`)
- `language`: Per-question (should be per-batch)

#### V2 Required Fields
```json
{
  "type": "MultipleChoice",
  "content": "question text",
  "target_level": 3,
  "difficulty": "Medium",
  "points": 10
}
```
**Benefits**:
- ✅ Field names match DB exactly
- ✅ No unnecessary `id` (DB generates)
- ✅ `target_level` added (critical for SFIA)
- ✅ Language in metadata (not per-question)

---

### 3. Multiple Choice Questions

#### V1 Format
```json
{
  "type": "mcq",
  "choices": [
    {
      "id": "A",
      "text": "Choice text",
      "is_correct": true,
      "explanation": "Why correct"
    }
  ],
  "shuffle_choices": true
}
```

#### V2 Format
```json
{
  "type": "MultipleChoice",
  "options": [
    {
      "content": "Choice text",
      "is_correct": true,
      "display_order": 1,
      "explanation": "Why correct"
    }
  ]
}
```

**Improvements**:
- ✅ `choices` → `options` (matches DB)
- ✅ `text` → `content` (matches DB)
- ✅ `display_order` replaces random `id`
- ✅ Removed `shuffle_choices` (handled by frontend)

---

### 4. True/False Questions

#### V1 Format
```json
{
  "type": "true_false",
  "answer": true
}
```
**Issues**:
- ❌ Boolean answer (không có explanation)
- ❌ Không có options structure

#### V2 Format
```json
{
  "type": "TrueFalse",
  "options": [
    {
      "content": "True",
      "is_correct": false,
      "display_order": 1,
      "explanation": "Why false"
    },
    {
      "content": "False",
      "is_correct": true,
      "display_order": 2,
      "explanation": "Why true"
    }
  ]
}
```

**Benefits**:
- ✅ Consistent structure with other choice types
- ✅ Có explanation cho cả 2 choices
- ✅ Easy to import to DB

---

### 5. Short Answer

#### V1 Format
```json
{
  "type": "short_answer",
  "expected_answer": "Answer text",
  "rubric": [
    {
      "criteria": "Mentions X",
      "points": 5
    }
  ]
}
```

#### V2 Format
```json
{
  "type": "ShortAnswer",
  "grading_rubric": "{\"criteria\":[{\"keyword\":\"X\",\"points\":5}],\"min_words\":10,\"max_words\":100}"
}
```

**Improvements**:
- ✅ Unified `grading_rubric` (JSON string cho DB)
- ✅ Added word count constraints
- ✅ Keyword-based grading support

---

### 6. Coding Challenges

#### V1 Format
```json
{
  "type": "coding",
  "coding_language": "python",
  "starter_code": "...",
  "test_cases": [
    {
      "input": "5",
      "expected_output": "120",
      "hidden": false
    }
  ],
  "max_runtime_seconds": 5
}
```
**Issues**:
- ❌ Flat structure (fields scattered)
- ❌ `max_runtime_seconds` separate from test cases

#### V2 Format
```json
{
  "type": "CodingChallenge",
  "code_snippet": "// Starter code",
  "grading_rubric": "{\"test_cases\":[{\"input\":\"5\",\"expected_output\":\"120\",\"points\":5}],\"code_quality\":{\"time_complexity\":\"O(n)\"}}"
}
```

**Benefits**:
- ✅ `starter_code` → `code_snippet` (matches DB)
- ✅ Test cases in `grading_rubric` (unified format)
- ✅ Added code quality criteria
- ✅ Points per test case

---

### 7. Missing Types in V1

#### Scenario
**V1**: ❌ Not supported (would map to "essay")

**V2**: ✅ Dedicated type
```json
{
  "type": "Scenario",
  "content": "Complex situation...",
  "grading_rubric": "{\"criteria\":[{\"name\":\"Analysis\",\"points\":10}]}"
}
```

#### Situational Judgment (SJT)
**V1**: ❌ Not supported

**V2**: ✅ Full support
```json
{
  "type": "SituationalJudgment",
  "options": [
    {
      "content": "Action A",
      "effectiveness_level": "MostEffective",
      "is_correct": true
    }
  ]
}
```

#### Rating
**V1**: ❌ Not supported

**V2**: ✅ Dedicated type
```json
{
  "type": "Rating",
  "options": [
    {"content": "1 - Low", "is_correct": true},
    {"content": "5 - High", "is_correct": true}
  ]
}
```

---

### 8. Auto-grading Support

#### V1 Auto-grading
| Type | Auto-gradable | Note |
|------|---------------|------|
| MCQ | ✅ Yes | Has `is_correct` |
| True/False | ✅ Yes | Has `answer` |
| Short Answer | ⚠️ Partial | Has `expected_answer` but vague |
| Essay | ❌ No | Manual only |
| Coding | ✅ Yes | Has test cases |

**Issues**:
- Short answer: Exact match only, no keyword scoring
- No support for partial credit

#### V2 Auto-grading
| Type | Auto-gradable | Scoring Method |
|------|---------------|----------------|
| MultipleChoice | ✅ Yes | Check `is_correct` |
| MultipleAnswer | ✅ Yes | Partial credit formula |
| TrueFalse | ✅ Yes | Check `is_correct` |
| ShortAnswer | ⚠️ Semi-auto | Keyword matching |
| LongAnswer | ❌ Manual | With AI assistance |
| CodingChallenge | ✅ Yes | Test case execution |
| Scenario | ❌ Manual | With criteria |
| SJT | ✅ Yes | Effectiveness-based scoring |
| Rating | ✅ Yes | All valid (self-assessment) |

**Improvements**:
- ✅ Partial credit for MultipleAnswer
- ✅ Keyword scoring for ShortAnswer
- ✅ SJT effectiveness levels
- ✅ Criteria-based rubrics

---

### 9. Database Integration

#### V1 → DB (Manual Mapping Required)
```csharp
// Need to manually map fields
var question = new CreateQuestionDto
{
    Type = MapType(v1.Type),           // "mcq" → QuestionType.MultipleChoice
    Content = v1.Stem,                 // "stem" → "content"
    TargetLevel = ???,                 // Missing in V1!
    Difficulty = MapDifficulty(v1.Difficulty),
    Points = v1.Points ?? 1,
    Options = v1.Choices?.Select(c => new QuestionOptionDto
    {
        Content = c.Text,              // "text" → "content"
        IsCorrect = c.IsCorrect,
        DisplayOrder = ExtractOrder(c.Id) // "A" → 1
    }).ToList()
};
```

**Issues**:
- ❌ Field name mismatches
- ❌ Missing `target_level` (critical!)
- ❌ Type enum conversion needed
- ❌ Manual parsing of choice IDs

#### V2 → DB (Direct Copy)
```csharp
// Almost 1-1 mapping!
var question = new CreateQuestionDto
{
    SkillId = Guid.Parse(v2.SkillId),
    Type = Enum.Parse<QuestionType>(v2.Type),
    Content = v2.Content,
    CodeSnippet = v2.CodeSnippet,
    MediaUrl = v2.MediaUrl,
    TargetLevel = (ProficiencyLevel)v2.TargetLevel,
    Difficulty = Enum.Parse<DifficultyLevel>(v2.Difficulty),
    Points = v2.Points,
    TimeLimitSeconds = v2.TimeLimitSeconds,
    Tags = v2.Tags,
    GradingRubric = v2.GradingRubric,
    Options = v2.Options?.Select(o => new CreateQuestionOptionDto
    {
        Content = o.Content,
        IsCorrect = o.IsCorrect,
        DisplayOrder = o.DisplayOrder,
        Explanation = o.Explanation
    }).ToList()
};
```

**Benefits**:
- ✅ Field names match exactly
- ✅ Enum values match exactly
- ✅ No conversion logic needed
- ✅ Minimal code

---

### 10. Metadata

#### V1 Metadata
```json
{
  "questions": [...],
  "metadata": {}  // Empty or minimal
}
```

#### V2 Metadata
```json
{
  "questions": [...],
  "metadata": {
    "total_questions": 10,
    "generation_timestamp": "2026-01-23T10:30:00Z",
    "ai_model": "gemini-2.0-flash-exp",
    "skill_id": "uuid",
    "skill_name": "Skill Name",
    "language": "en"
  }
}
```

**Benefits**:
- ✅ Tracking: Timestamp, model used
- ✅ Context: Skill info, language
- ✅ Validation: Total count check

---

## Migration Guide

### For Existing V1 Data

```python
def migrate_v1_to_v2(v1_question):
    """Convert V1 format to V2 format"""

    # Type mapping
    type_map = {
        "mcq": "MultipleChoice",
        "true_false": "TrueFalse",
        "short_answer": "ShortAnswer",
        "essay": "LongAnswer",
        "coding": "CodingChallenge"
    }

    # Difficulty mapping
    diff_map = {
        "easy": "Easy",
        "medium": "Medium",
        "hard": "Hard"
    }

    v2_question = {
        "type": type_map[v1_question["type"]],
        "content": v1_question["stem"],
        "target_level": 3,  # Default - NEED TO DETERMINE!
        "difficulty": diff_map[v1_question["difficulty"]],
        "points": v1_question.get("points", 1),
        "time_limit_seconds": v1_question.get("time_limit_seconds"),
        "tags": v1_question.get("tags"),
        "explanation": v1_question.get("explanation")
    }

    # Handle choices → options
    if "choices" in v1_question:
        v2_question["options"] = [
            {
                "content": c["text"],
                "is_correct": c["is_correct"],
                "display_order": ord(c["id"]) - ord('A') + 1,
                "explanation": c.get("explanation")
            }
            for c in v1_question["choices"]
        ]

    # Handle true/false
    elif v1_question["type"] == "true_false":
        v2_question["options"] = [
            {
                "content": "True",
                "is_correct": v1_question["answer"],
                "display_order": 1
            },
            {
                "content": "False",
                "is_correct": not v1_question["answer"],
                "display_order": 2
            }
        ]

    # Handle rubric → grading_rubric
    if "rubric" in v1_question:
        v2_question["grading_rubric"] = json.dumps({
            "criteria": v1_question["rubric"]
        })

    # Handle coding test cases
    if "test_cases" in v1_question:
        v2_question["grading_rubric"] = json.dumps({
            "test_cases": v1_question["test_cases"],
            "language": v1_question.get("coding_language", "javascript")
        })
        v2_question["code_snippet"] = v1_question.get("starter_code")

    return v2_question
```

---

## Recommendation

### ✅ Use V2 Schema

**Reasons**:
1. **Direct DB integration** - No mapping needed
2. **Complete type support** - All 9 types
3. **Better auto-grading** - Partial credit, keyword matching
4. **Production-ready** - Matches backend DTOs
5. **Extensible** - Easy to add new fields

### ⚠️ V1 Issues
1. Missing 4 question types
2. Field name mismatches with DB
3. Missing `target_level` (critical for SFIA)
4. Incomplete auto-grading support
5. Requires conversion code

---

## Summary Table

| Feature | V1 | V2 | Winner |
|---------|----|----|--------|
| Question Types | 5 | 9 | ✅ V2 |
| DB Mapping | Indirect | Direct | ✅ V2 |
| Field Names | Generic | DB-matching | ✅ V2 |
| Auto-grading | Partial | Complete | ✅ V2 |
| SFIA Support | ❌ No target_level | ✅ target_level | ✅ V2 |
| SJT Support | ❌ No | ✅ effectiveness_level | ✅ V2 |
| Code Complexity | Simple | Simple | 🟰 Tie |
| Documentation | 4.5 KB | 25 KB | ✅ V2 |
| Examples | 3 | 9 (all types) | ✅ V2 |

**Verdict**: V2 wins 8/9 categories

---

## Files

- **V1 Schema**: `output_question_schema.json` (old)
- **V2 Schema**: `output_question_schema_v2.json` ✨ (recommended)
- **V2 Guide**: `OUTPUT_SCHEMA_V2_GUIDE.md`
- **Comparison**: `SCHEMA_V1_VS_V2_COMPARISON.md` (this file)
