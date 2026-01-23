# Output Schema V2 Guide

## Overview

Schema mới đơn giản hơn, map trực tiếp với `CreateQuestionDto` trong backend, dễ dàng import vào database.

**File**: `src/schemas/output_question_schema_v2.json`

---

## Key Improvements

### ✅ So với Schema Cũ

| Feature | Old Schema | New Schema V2 |
|---------|------------|---------------|
| **Mapping to DB** | ❌ Không trực tiếp | ✅ Map 1-1 với `CreateQuestionDto` |
| **Auto-grading** | ⚠️ Thiếu answer cho một số types | ✅ Có answer/criteria cho tất cả |
| **Complexity** | ❌ Nhiều fields không cần thiết | ✅ Chỉ giữ fields quan trọng |
| **Question Types** | 5 types | ✅ 9 types (full support) |
| **SJT Support** | ❌ Không có | ✅ Có `effectiveness_level` |
| **Grading Rubric** | Scattered | ✅ Unified `grading_rubric` JSON |
| **Coding Challenges** | Separate test_cases | ✅ Test cases trong `grading_rubric` |

---

## Schema Structure

### Top Level

```json
{
  "questions": [...],  // Array of questions
  "metadata": {        // Generation metadata
    "total_questions": 10,
    "generation_timestamp": "2026-01-23T10:30:00Z",
    "ai_model": "gemini-2.0-flash-exp",
    "skill_id": "uuid",
    "skill_name": "Skill Name",
    "language": "en"
  }
}
```

### Question Object (Core Fields)

```json
{
  "skill_id": "uuid",              // Question.SkillId
  "type": "MultipleChoice",        // QuestionType enum
  "content": "Question text",      // Question.Content
  "code_snippet": "...",           // Question.CodeSnippet (nullable)
  "media_url": "https://...",      // Question.MediaUrl (nullable)
  "target_level": 3,               // Question.TargetLevel (1-7)
  "difficulty": "Medium",          // DifficultyLevel enum
  "points": 10,                    // Question.Points
  "time_limit_seconds": 120,       // Question.TimeLimitSeconds (nullable)
  "tags": ["tag1", "tag2"],        // Question.Tags (nullable)
  "options": [...],                // QuestionOption[] (for choice questions)
  "grading_rubric": "{...}",       // Question.GradingRubric JSON string
  "explanation": "...",            // Explanation for learners
  "hints": ["hint1", "hint2"]      // Helpful hints
}
```

---

## Question Types & Requirements

### 1. MultipleChoice (1 đáp án đúng)

**Required**: `options` (min 2 items, exactly 1 có `is_correct: true`)

```json
{
  "type": "MultipleChoice",
  "content": "Which is correct?",
  "target_level": 3,
  "difficulty": "Medium",
  "points": 10,
  "options": [
    {
      "content": "Option A",
      "is_correct": true,
      "display_order": 1,
      "explanation": "Why this is correct"
    },
    {
      "content": "Option B",
      "is_correct": false,
      "display_order": 2
    }
  ]
}
```

**Auto-grading**: Check if selected option has `is_correct: true`

---

### 2. MultipleAnswer (Nhiều đáp án đúng)

**Required**: `options` (min 2 items, ≥1 có `is_correct: true`)

```json
{
  "type": "MultipleAnswer",
  "content": "Select all that apply",
  "points": 15,
  "options": [
    {"content": "Correct 1", "is_correct": true, "display_order": 1},
    {"content": "Correct 2", "is_correct": true, "display_order": 2},
    {"content": "Wrong", "is_correct": false, "display_order": 3}
  ]
}
```

**Auto-grading**:
- Full points: Chọn đúng TẤT CẢ correct options, không chọn wrong options
- Partial: Points × (correct_selected / total_correct) - (wrong_selected × penalty)

---

### 3. TrueFalse

**Required**: `options` (exactly 2: True & False)

```json
{
  "type": "TrueFalse",
  "content": "Statement is true or false?",
  "points": 5,
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

**Auto-grading**: Check selected option `is_correct`

---

### 4. ShortAnswer

**Required**: `grading_rubric` (JSON string)

```json
{
  "type": "ShortAnswer",
  "content": "What is...?",
  "points": 5,
  "grading_rubric": "{\"criteria\":[{\"keyword\":\"key term\",\"points\":2},{\"keyword\":\"concept\",\"points\":3}],\"min_words\":10,\"max_words\":100}"
}
```

**Grading Rubric Format**:
```json
{
  "criteria": [
    {
      "keyword": "string to search for",
      "points": 2,
      "case_sensitive": false
    }
  ],
  "min_words": 10,
  "max_words": 100,
  "exact_match": "optional exact answer"
}
```

**Auto-grading**:
- Keyword matching (full/partial credit)
- Word count validation
- Optional exact match

---

### 5. LongAnswer (Essay)

**Required**: `grading_rubric` (JSON string)

```json
{
  "type": "LongAnswer",
  "content": "Explain in detail...",
  "points": 20,
  "time_limit_seconds": 600,
  "grading_rubric": "{\"criteria\":[{\"name\":\"Clarity\",\"points\":5,\"description\":\"Clear structure\"},{\"name\":\"Examples\",\"points\":5,\"description\":\"Provides 2+ examples\"}],\"min_words\":150,\"max_words\":500}"
}
```

**Grading Rubric Format**:
```json
{
  "criteria": [
    {
      "name": "Criterion name",
      "points": 5,
      "description": "What to look for",
      "keywords": ["optional", "keywords"]
    }
  ],
  "min_words": 150,
  "max_words": 500
}
```

**Grading**: Manual/Semi-automated với AI assistance

---

### 6. CodingChallenge

**Required**: `grading_rubric` (JSON string with test cases)

```json
{
  "type": "CodingChallenge",
  "content": "Write a function that...",
  "code_snippet": "// Starter code\nfunction solution() {\n  // Your code\n}",
  "points": 25,
  "time_limit_seconds": 900,
  "tags": ["JavaScript", "Algorithms"],
  "grading_rubric": "{\"test_cases\":[{\"input\":\"5\",\"expected_output\":\"120\",\"points\":5,\"description\":\"Factorial of 5\"},{\"input\":\"0\",\"expected_output\":\"1\",\"points\":5,\"description\":\"Edge case: 0\"}],\"code_quality\":{\"no_hardcode\":5,\"time_complexity\":\"O(n)\"}}"
}
```

**Grading Rubric Format**:
```json
{
  "test_cases": [
    {
      "input": "test input",
      "expected_output": "expected result",
      "points": 5,
      "description": "Test case description",
      "hidden": false
    }
  ],
  "code_quality": {
    "no_hardcode": 5,
    "time_complexity": "O(n)",
    "space_complexity": "O(1)"
  },
  "language": "javascript"
}
```

**Auto-grading**: Run code against test cases

---

### 7. Scenario

**Required**: `grading_rubric` (JSON string)

```json
{
  "type": "Scenario",
  "content": "You are in a situation where... How would you handle this?",
  "points": 30,
  "time_limit_seconds": 900,
  "grading_rubric": "{\"criteria\":[{\"name\":\"Stakeholder analysis\",\"points\":10},{\"name\":\"Action plan\",\"points\":10},{\"name\":\"Risk mitigation\",\"points\":10}],\"min_words\":200,\"max_words\":500}"
}
```

**Grading**: Similar to LongAnswer với criteria-based rubric

---

### 8. SituationalJudgment (SJT)

**Required**: `options` với `effectiveness_level`

```json
{
  "type": "SituationalJudgment",
  "content": "A team member... What would you do?",
  "points": 15,
  "options": [
    {
      "content": "Action A",
      "is_correct": true,
      "display_order": 1,
      "effectiveness_level": "MostEffective",
      "explanation": "This is best because..."
    },
    {
      "content": "Action B",
      "is_correct": false,
      "display_order": 2,
      "effectiveness_level": "Effective"
    },
    {
      "content": "Action C",
      "is_correct": false,
      "display_order": 3,
      "effectiveness_level": "Ineffective"
    },
    {
      "content": "Action D",
      "is_correct": false,
      "display_order": 4,
      "effectiveness_level": "CounterProductive"
    }
  ]
}
```

**Effectiveness Levels** (từ SjtEffectiveness enum):
- `MostEffective` - Best choice (full points)
- `Effective` - Good choice (75% points)
- `Ineffective` - Poor choice (25% points)
- `CounterProductive` - Worst choice (0 points)

**Auto-grading**: Points based on effectiveness_level

---

### 9. Rating (Self-assessment)

**Required**: `options` (3-10 items, all `is_correct: true`)

```json
{
  "type": "Rating",
  "content": "How confident are you with...?",
  "points": 5,
  "options": [
    {"content": "1 - Not confident", "is_correct": true, "display_order": 1},
    {"content": "2 - Slightly confident", "is_correct": true, "display_order": 2},
    {"content": "3 - Moderately confident", "is_correct": true, "display_order": 3},
    {"content": "4 - Very confident", "is_correct": true, "display_order": 4},
    {"content": "5 - Extremely confident", "is_correct": true, "display_order": 5}
  ]
}
```

**Grading**: All answers valid (self-assessment), full points awarded

---

## Field Mappings to Database

### Question Entity Mapping

| Schema Field | Database Field | Type | Notes |
|--------------|----------------|------|-------|
| `skill_id` | `Question.SkillId` | Guid? | Nullable |
| `type` | `Question.Type` | QuestionType enum | 9 values |
| `content` | `Question.Content` | string | Required |
| `code_snippet` | `Question.CodeSnippet` | string? | Nullable |
| `media_url` | `Question.MediaUrl` | string? | Nullable |
| `target_level` | `Question.TargetLevel` | ProficiencyLevel enum | 1-7 |
| `difficulty` | `Question.Difficulty` | DifficultyLevel enum | Easy/Medium/Hard/Expert |
| `points` | `Question.Points` | int | Default 1 |
| `time_limit_seconds` | `Question.TimeLimitSeconds` | int? | Nullable |
| `tags` | `Question.Tags` | List<string>? | JSON array |
| `grading_rubric` | `Question.GradingRubric` | string? | JSON string |

### QuestionOption Entity Mapping

| Schema Field | Database Field | Type |
|--------------|----------------|------|
| `content` | `QuestionOption.Content` | string |
| `is_correct` | `QuestionOption.IsCorrect` | bool |
| `display_order` | `QuestionOption.DisplayOrder` | int |
| `explanation` | `QuestionOption.Explanation` | string? |

---

## Import to Database Example

### C# Code

```csharp
using SkillMatrix.Application.DTOs.Assessment;
using System.Text.Json;

// Parse AI output
var aiOutput = JsonSerializer.Deserialize<AiGeneratedOutput>(jsonString);

foreach (var q in aiOutput.Questions)
{
    var createDto = new CreateQuestionDto
    {
        SkillId = Guid.Parse(q.SkillId),
        Type = Enum.Parse<QuestionType>(q.Type),
        Content = q.Content,
        CodeSnippet = q.CodeSnippet,
        MediaUrl = q.MediaUrl,
        TargetLevel = (ProficiencyLevel)q.TargetLevel,
        Difficulty = Enum.Parse<DifficultyLevel>(q.Difficulty),
        Points = q.Points,
        TimeLimitSeconds = q.TimeLimitSeconds,
        Tags = q.Tags,
        GradingRubric = q.GradingRubric,
        Options = q.Options?.Select(o => new CreateQuestionOptionDto
        {
            Content = o.Content,
            IsCorrect = o.IsCorrect,
            DisplayOrder = o.DisplayOrder,
            Explanation = o.Explanation
        }).ToList() ?? new()
    };

    // Save to database
    await _questionService.CreateQuestionAsync(createDto);
}
```

---

## Validation Rules

### Required for All Types
- ✅ `type`
- ✅ `content` (min 10 chars)
- ✅ `target_level` (1-7)
- ✅ `difficulty`
- ✅ `points` (min 1)

### Type-Specific Requirements

| Type | Required Fields |
|------|----------------|
| MultipleChoice | `options` (min 2, 1 correct) |
| MultipleAnswer | `options` (min 2, ≥1 correct) |
| TrueFalse | `options` (exactly 2) |
| ShortAnswer | `grading_rubric` |
| LongAnswer | `grading_rubric` |
| CodingChallenge | `grading_rubric` |
| Scenario | `grading_rubric` |
| SituationalJudgment | `options` with `effectiveness_level` |
| Rating | `options` (3-10, all correct) |

---

## Complete Examples

Xem trong schema file `output_question_schema_v2.json`, phần `examples` có 9 câu hỏi mẫu cho tất cả 9 types.

---

## Summary

### ✅ Benefits

1. **Direct DB Mapping** - Copy-paste vào CreateQuestionDto
2. **Auto-grading Ready** - Tất cả types có answer/criteria
3. **Simplified** - Chỉ giữ fields cần thiết
4. **Consistent** - Unified grading_rubric format
5. **Type-safe** - Match với backend enums
6. **Extensible** - Dễ thêm fields mới

### 📊 Stats

- **Total Types**: 9
- **Auto-gradable**: 6 (MCQ, MA, TF, SJT, Rating, Coding)
- **Manual/Semi-auto**: 3 (Short, Long, Scenario)
- **Average fields per question**: 10-15 (vs 20+ in old schema)

---

## Next Steps

1. Update AI generator to produce this format
2. Create validator for output schema v2
3. Create import service in backend
4. Update frontend to display all 9 types
5. Implement auto-grading logic
