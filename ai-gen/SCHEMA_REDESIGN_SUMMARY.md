# Schema Redesign Summary

## 🎯 Mục Tiêu

Thiết kế lại output schema để:
1. ✅ Map trực tiếp với backend `CreateQuestionDto`
2. ✅ Support đầy đủ 9 loại câu hỏi
3. ✅ Có answer/criteria cho auto-grading
4. ✅ Đơn giản, dễ hiểu, dễ maintain

---

## 📄 Files Mới Tạo

### 1. Schema V2
- ✅ `src/schemas/output_question_schema_v2.json` (25 KB)
  - 9 question types đầy đủ
  - Direct mapping với CreateQuestionDto
  - 9 examples (1 cho mỗi type)

### 2. Documentation (2 files, 20 KB)
- ✅ `OUTPUT_SCHEMA_V2_GUIDE.md` (13 KB)
  - Chi tiết từng question type
  - Grading rubric formats
  - DB mapping guide
  - Import code examples

- ✅ `SCHEMA_V1_VS_V2_COMPARISON.md` (7 KB)
  - So sánh chi tiết V1 vs V2
  - Migration guide
  - Verdict: V2 wins 8/9 categories

---

## 🆚 V1 vs V2 Quick Comparison

| Aspect | V1 (Old) | V2 (New) |
|--------|----------|----------|
| **Question Types** | 5 types | ✅ 9 types |
| **DB Integration** | ❌ Requires mapping | ✅ Direct copy |
| **Field Names** | Generic (`stem`, `choices`) | ✅ DB-matching (`content`, `options`) |
| **SFIA Support** | ❌ No `target_level` | ✅ Has `target_level` |
| **Auto-grading** | ⚠️ Partial | ✅ Complete |
| **SJT Support** | ❌ No | ✅ `effectiveness_level` |
| **Grading Rubric** | Scattered | ✅ Unified JSON format |

---

## 📊 Question Types Coverage

### V1 Schema (5 types)
- ✅ MCQ
- ✅ True/False
- ✅ Short Answer
- ✅ Essay
- ✅ Coding

**Missing**: MultipleAnswer, Scenario, SJT, Rating

### V2 Schema (9 types) ✅
1. **MultipleChoice** - 1 đáp án đúng
2. **MultipleAnswer** - Nhiều đáp án đúng
3. **TrueFalse** - Đúng/Sai
4. **ShortAnswer** - Câu trả lời ngắn
5. **LongAnswer** - Essay
6. **CodingChallenge** - Coding
7. **Scenario** - Tình huống
8. **SituationalJudgment** - SJT với effectiveness levels
9. **Rating** - Self-assessment scale

---

## 🔄 Direct DB Mapping

### V2 Schema → CreateQuestionDto

```csharp
// Minimal conversion code needed!
var dto = new CreateQuestionDto
{
    SkillId = Guid.Parse(question.SkillId),
    Type = Enum.Parse<QuestionType>(question.Type),
    Content = question.Content,
    CodeSnippet = question.CodeSnippet,
    MediaUrl = question.MediaUrl,
    TargetLevel = (ProficiencyLevel)question.TargetLevel,
    Difficulty = Enum.Parse<DifficultyLevel>(question.Difficulty),
    Points = question.Points,
    TimeLimitSeconds = question.TimeLimitSeconds,
    Tags = question.Tags,
    GradingRubric = question.GradingRubric,
    Options = question.Options?.Select(o => new CreateQuestionOptionDto
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
- ✅ No complex transformation
- ✅ Type-safe

---

## 🎓 Auto-grading Support

### Choice-Based (100% Auto)
| Type | Method | Partial Credit |
|------|--------|----------------|
| MultipleChoice | Check `is_correct` | ❌ No |
| MultipleAnswer | Count correct selections | ✅ Yes |
| TrueFalse | Check `is_correct` | ❌ No |
| SJT | `effectiveness_level` points | ✅ Yes |
| Rating | All valid | N/A |

### Text-Based (Semi-auto)
| Type | Method | AI Assistance |
|------|--------|---------------|
| ShortAnswer | Keyword matching | ✅ Optional |
| LongAnswer | Criteria + manual | ✅ Yes |
| Scenario | Criteria + manual | ✅ Yes |

### Code-Based (100% Auto)
| Type | Method |
|------|--------|
| CodingChallenge | Test case execution |

---

## 📝 Example: MultipleChoice

```json
{
  "type": "MultipleChoice",
  "content": "Which WCAG level requires keyboard access?",
  "target_level": 3,
  "difficulty": "Medium",
  "points": 10,
  "time_limit_seconds": 120,
  "tags": ["WCAG", "Accessibility"],
  "options": [
    {
      "content": "Level A",
      "is_correct": true,
      "display_order": 1,
      "explanation": "Level A is the minimum level"
    },
    {
      "content": "Level AA",
      "is_correct": false,
      "display_order": 2
    }
  ],
  "explanation": "WCAG Level A requires basic accessibility."
}
```

**Auto-grading**: Check if selected option has `is_correct: true` → 10 points

---

## 📝 Example: SituationalJudgment

```json
{
  "type": "SituationalJudgment",
  "content": "A team member submits late work. What would you do?",
  "points": 15,
  "options": [
    {
      "content": "Schedule formal review",
      "is_correct": true,
      "display_order": 1,
      "effectiveness_level": "MostEffective",
      "explanation": "Best approach"
    },
    {
      "content": "Assign less critical tasks",
      "is_correct": false,
      "display_order": 2,
      "effectiveness_level": "Ineffective"
    }
  ]
}
```

**Auto-grading**:
- MostEffective → 100% points (15)
- Effective → 75% points (11.25)
- Ineffective → 25% points (3.75)
- CounterProductive → 0 points

---

## 📝 Example: CodingChallenge

```json
{
  "type": "CodingChallenge",
  "content": "Write a function to check valid email",
  "code_snippet": "function isValidEmail(email) {\n  // Your code\n}",
  "points": 25,
  "time_limit_seconds": 900,
  "grading_rubric": "{\"test_cases\":[{\"input\":\"test@example.com\",\"expected_output\":true,\"points\":5},{\"input\":\"invalid\",\"expected_output\":false,\"points\":5}],\"code_quality\":{\"uses_regex\":5}}"
}
```

**Auto-grading**: Execute code, run test cases, sum points

---

## 🔑 Key Improvements

### 1. Unified Grading Rubric
**V1**: Scattered (`rubric`, `test_cases`, `expected_answer` in different places)

**V2**: Unified `grading_rubric` JSON string
```json
{
  "grading_rubric": "{\"criteria\":[...],\"test_cases\":[...],\"code_quality\":{...}}"
}
```

### 2. SFIA Integration
**V1**: ❌ No `target_level`

**V2**: ✅ `target_level` (1-7) maps to ProficiencyLevel enum

### 3. SJT Support
**V1**: ❌ Not supported

**V2**: ✅ `effectiveness_level` for nuanced scoring
- MostEffective (100%)
- Effective (75%)
- Ineffective (25%)
- CounterProductive (0%)

### 4. Consistent Option Structure
**V2**: Tất cả choice-based questions dùng cùng format:
```json
{
  "content": "text",
  "is_correct": true,
  "display_order": 1,
  "explanation": "why"
}
```

---

## 📚 Complete Documentation

| Document | Size | Purpose |
|----------|------|---------|
| `output_question_schema_v2.json` | 25 KB | Schema definition + 9 examples |
| `OUTPUT_SCHEMA_V2_GUIDE.md` | 13 KB | Detailed guide for all 9 types |
| `SCHEMA_V1_VS_V2_COMPARISON.md` | 7 KB | Migration guide, comparison |
| `SCHEMA_REDESIGN_SUMMARY.md` | This file | Quick reference |

---

## ✅ Ready for Use

### Backend Integration
1. Parse JSON output
2. Loop through `questions` array
3. Create `CreateQuestionDto` for each
4. Call `QuestionService.CreateQuestionAsync()`

### Frontend Display
- 9 question type components
- Answer options with explanations
- Time limits, points display
- Tags for filtering

### Auto-grading Engine
- Choice-based: Check `is_correct`
- SJT: Use `effectiveness_level`
- Text: Parse `grading_rubric` for keyword matching
- Code: Execute against test cases from `grading_rubric`

---

## 🎯 Recommendation

**USE V2 SCHEMA** ✅

**Reasons**:
1. Direct DB integration (no mapping needed)
2. Complete type coverage (9/9)
3. Production-ready (matches backend DTOs)
4. Better auto-grading (partial credit, SJT)
5. Future-proof (extensible design)

---

## 📞 Quick Reference

### Generate Questions
1. **Input**: `input_request_schema.json`
2. **AI Processing**: Generate questions
3. **Output**: `output_question_schema_v2.json`
4. **Import**: Direct to `CreateQuestionDto`

### File Locations
```
ai-gen/
├── src/schemas/
│   ├── input_request_schema.json         ← Request format
│   ├── output_question_schema_v2.json    ← Output format ✨
│   └── output_question_schema.json       ← Old (deprecated)
└── docs/
    ├── OUTPUT_SCHEMA_V2_GUIDE.md         ← Detailed guide
    ├── SCHEMA_V1_VS_V2_COMPARISON.md     ← V1 vs V2
    └── SCHEMA_REDESIGN_SUMMARY.md        ← This file
```

---

## 🚀 Next Steps

1. ✅ **Completed**: Schema V2 design & documentation
2. **TODO**: Update AI generator to output V2 format
3. **TODO**: Create backend import service
4. **TODO**: Update frontend to display all 9 types
5. **TODO**: Implement auto-grading engine
6. **TODO**: Add validation for output schema V2
