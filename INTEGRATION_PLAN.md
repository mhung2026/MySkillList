# AI Question Generation Integration Plan

**Created**: 2026-01-23
**Status**: Ready for Implementation

---

## 🎯 Goal

Gắn nút "Generate Questions" trong UI với AI generation service (FastAPI Python)

---

## 📊 Current State Analysis

### 1. Frontend UI ✅ READY

**File**: `web/src/pages/tests/TestTemplateDetail.tsx`
**Line**: 784-790 - Button "Generate Questions"

**Form Fields** (Lines 688-779):
```typescript
{
  questionTypes: QuestionType[],      // Checkbox group (line 699)
  language: 'en' | 'vi',              // Radio (line 708)
  questionCount: number,              // Number input (line 719)
  skillId?: string,                   // Select (line 734) SINGLE
  targetLevel?: ProficiencyLevel,     // Select (line 751) SINGLE
  difficulty?: DifficultyLevel,       // Select (line 763)
  additionalContext?: string          // Textarea (line 775)
}
```

**API Call**: Line 40, 169-177
```typescript
import { generateAiQuestions } from '../../api/testTemplates';

generateAiMutation.mutate({
  ...values,
  sectionId: selectedSectionId
});
```

---

### 2. Frontend API Client ✅ READY

**File**: `web/src/api/testTemplates.ts`
**Lines**: 90-98

```typescript
export const generateAiQuestions = async (
  data: GenerateAiQuestionsRequest
): Promise<QuestionDto[]> => {
  const response = await apiClient.post<QuestionDto[]>(
    '/questions/generate-ai',  // Backend endpoint
    data
  );
  return response.data;
};
```

**Request Type** (`web/src/types/index.ts:589`):
```typescript
export interface AiGenerateQuestionsRequest {
  questionTypes: QuestionType[];
  language: string;
  questionCount: number;
  skillId?: string;              // SINGLE skill
  targetLevel?: ProficiencyLevel; // SINGLE level
  difficulty?: DifficultyLevel;
  additionalContext?: string;
  sectionId?: string;
}
```

---

### 3. Backend C# API ⚠️ NEEDS UPDATE

**File**: `src/SkillMatrix.Api/Controllers/QuestionsController.cs`
**Lines**: 109-136

**Current Endpoint**:
```csharp
[HttpPost("generate-ai")]
public async Task<ActionResult<List<QuestionDto>>> GenerateFromAi(
    [FromBody] GenerateAiQuestionsRequest request)
{
    // Currently calls _service.CreateFromAiAsync()
    // NOT calling Python AI service yet
}
```

**Current DTO** (`src/SkillMatrix.Application/DTOs/Assessment/AiGenerationDto.cs`):
```csharp
public class AiGenerateQuestionsRequest
{
    public Guid? SkillId { get; set; }         // SINGLE
    public string? SkillName { get; set; }
    public ProficiencyLevel? TargetLevel { get; set; }  // SINGLE
    public int QuestionCount { get; set; } = 5;
    public AssessmentType AssessmentType { get; set; }
    public List<QuestionType> QuestionTypes { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public string? Language { get; set; }
    public string? AdditionalContext { get; set; }
}
```

---

### 4. Python AI Service ✅ SCHEMA READY, ❌ NOT DEPLOYED

**Files Created**:
- ✅ `ai-gen/src/schemas/input_request_schema.json`
- ✅ `ai-gen/src/schemas/output_question_schema_v2.json`
- ✅ `ai-gen/src/validators/request_validator.py`
- ✅ `ai-gen/db_skill_reader.py`

**Expected Input**:
```json
{
  "question_type": ["Multiple Choice", "Short Answer"],  // Array of user-friendly names
  "language": "English",                                 // User-friendly
  "number_of_questions": 10,
  "skills": [{                                           // ARRAY
    "skill_id": "uuid",
    "skill_name": "name"
  }],
  "target_proficiency_level": [3, 4],                   // ARRAY
  "difficulty": "Medium",                                // User-friendly
  "additional_context": "Focus on..."
}
```

**Output**:
```json
{
  "questions": [
    {
      "type": "MultipleChoice",
      "content": "Question text",
      "target_level": 3,
      "difficulty": "Medium",
      "points": 10,
      "options": [...]
    }
  ],
  "metadata": {...}
}
```

---

## 🔄 Integration Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. USER CLICKS "Generate Questions" Button (UI)            │
│    File: TestTemplateDetail.tsx:784-790                    │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Frontend Form Submit                                     │
│    Handler: handleAiGenerate (line 246)                    │
│    Request: {questionTypes, language, questionCount, etc}  │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. API Call                                                 │
│    POST /api/questions/generate-ai                         │
│    File: web/src/api/testTemplates.ts:90                   │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. C# Backend Receives Request                             │
│    Controller: QuestionsController.GenerateFromAi()        │
│    File: QuestionsController.cs:109                        │
│                                                             │
│    ⚠️ NEEDS TO BE UPDATED:                                 │
│    - Call Python AI service via HTTP                       │
│    - Transform request format                              │
│    - Map response to CreateQuestionDto                     │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Call Python FastAPI Service                             │
│    POST http://localhost:8002/api/v2/generate-questions    │
│                                                             │
│    ⚠️ NEEDS TO BE CREATED:                                 │
│    - Deploy FastAPI with new schema                        │
│    - Implement /api/v2/generate-questions endpoint         │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Python AI Service                                        │
│    - Validate input (request_validator.py)                 │
│    - Fetch skill data from DB (db_skill_reader.py)         │
│    - Generate questions with AI (Gemini)                   │
│    - Return output_question_schema_v2                      │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. C# Backend Maps Response                                │
│    - Parse AI output                                        │
│    - Create CreateQuestionDto for each question            │
│    - Save to database                                       │
│    - Return QuestionDto[] to frontend                      │
└─────────────────────┬───────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. Frontend Displays Results                               │
│    - Show success message                                   │
│    - Refresh question list                                  │
│    - Close modal                                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 Implementation Steps

### Step 1: Deploy Python AI Service ⏳ TODO

**1.1. Create `ai-gen/src/api/routes_v2.py`**

```python
from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
from datetime import datetime

from ..validators.request_validator import validate_and_normalize
from db_skill_reader import getSkillLevelsBySkillId
# from ..generators.question_generator_v2 import generate_questions_v2

router = APIRouter(prefix="/api/v2", tags=["AI Generation V2"])

class SkillInfo(BaseModel):
    skill_id: str
    skill_name: str
    skill_code: Optional[str] = None

class GenerateRequestV2(BaseModel):
    question_type: List[str]           # User-friendly names
    language: str                      # "English" or "Vietnamese"
    number_of_questions: int
    skills: Optional[List[SkillInfo]] = None
    target_proficiency_level: Optional[List[int]] = None
    difficulty: Optional[str] = None
    additional_context: Optional[str] = None

@router.post("/generate-questions")
async def generate_questions_v2(request: GenerateRequestV2):
    """Generate questions using V2 schema"""
    try:
        # 1. Validate and normalize
        is_valid, error, normalized = validate_and_normalize(request.dict())
        if not is_valid:
            raise HTTPException(status_code=422, detail=error)

        # 2. Fetch skill data from DB if provided
        skill_data = None
        if normalized.get("skills") and len(normalized["skills"]) > 0:
            skill_id = normalized["skills"][0]["skill_id"]
            levels = getSkillLevelsBySkillId(skill_id)
            # Format skill data...

        # 3. Generate questions with AI
        # questions = generate_questions_v2(normalized, skill_data)

        # 4. Return in output_question_schema_v2 format
        return {
            "questions": [],  # TODO: Implement generator
            "metadata": {
                "total_questions": normalized["number_of_questions"],
                "generation_timestamp": datetime.now().isoformat(),
                "ai_model": "gemini-2.0-flash-exp",
                "language": normalized["language"]
            }
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
```

**1.2. Update `ai-gen/main.py`**

```python
from src.api import routes_v2

# Add after existing middleware
app.include_router(routes_v2.router)
```

**1.3. Run FastAPI**

```bash
cd ai-gen
.venv\Scripts\python.exe main.py
# Should run on http://localhost:8002
```

---

### Step 2: Update C# Backend ⏳ TODO

**2.1. Add HttpClient Service**

Create `src/SkillMatrix.Infrastructure/Services/AiQuestionGeneratorClient.cs`:

```csharp
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SkillMatrix.Infrastructure.Services;

public interface IAiQuestionGeneratorClient
{
    Task<AiGeneratedQuestionsResponse> GenerateQuestionsAsync(AiGenerateRequestDto request);
}

public class AiQuestionGeneratorClient : IAiQuestionGeneratorClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AiQuestionGeneratorClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["AiService:BaseUrl"] ?? "http://localhost:8002";
    }

    public async Task<AiGeneratedQuestionsResponse> GenerateQuestionsAsync(
        AiGenerateRequestDto request)
    {
        var endpoint = $"{_baseUrl}/api/v2/generate-questions";

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AiGeneratedQuestionsResponse>(responseJson);
    }
}

// DTOs to match Python schema
public class AiGenerateRequestDto
{
    public List<string> QuestionType { get; set; }
    public string Language { get; set; }
    public int NumberOfQuestions { get; set; }
    public List<SkillInfoDto>? Skills { get; set; }
    public List<int>? TargetProficiencyLevel { get; set; }
    public string? Difficulty { get; set; }
    public string? AdditionalContext { get; set; }
}

public class SkillInfoDto
{
    public string SkillId { get; set; }
    public string SkillName { get; set; }
    public string? SkillCode { get; set; }
}

public class AiGeneratedQuestionsResponse
{
    public List<AiGeneratedQuestion> Questions { get; set; }
    public AiMetadata Metadata { get; set; }
}

public class AiGeneratedQuestion
{
    public string Type { get; set; }
    public string Content { get; set; }
    public int TargetLevel { get; set; }
    public string Difficulty { get; set; }
    public int Points { get; set; }
    public List<AiQuestionOption>? Options { get; set; }
    public string? GradingRubric { get; set; }
    // ... other fields
}
```

**2.2. Register in DI** (`src/SkillMatrix.Api/Program.cs`):

```csharp
// Add HTTP client for AI service
builder.Services.AddHttpClient<IAiQuestionGeneratorClient, AiQuestionGeneratorClient>();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json");
```

**2.3. Add to appsettings.json**:

```json
{
  "AiService": {
    "BaseUrl": "http://localhost:8002"
  }
}
```

**2.4. Update QuestionsController**:

```csharp
private readonly IAiQuestionGeneratorClient _aiClient;

public QuestionsController(
    IQuestionService service,
    IAiQuestionGeneratorClient aiClient)
{
    _service = service;
    _aiClient = aiClient;
}

[HttpPost("generate-ai")]
public async Task<ActionResult<List<QuestionDto>>> GenerateFromAi(
    [FromBody] GenerateAiQuestionsRequest request)
{
    try
    {
        // 1. Transform to Python AI service format
        var aiRequest = new AiGenerateRequestDto
        {
            QuestionType = request.QuestionTypes.Select(MapQuestionType).ToList(),
            Language = request.Language == "en" ? "English" : "Vietnamese",
            NumberOfQuestions = request.QuestionCount,
            Skills = request.SkillId.HasValue
                ? new List<SkillInfoDto>
                  {
                      new() { SkillId = request.SkillId.ToString() }
                  }
                : null,
            TargetProficiencyLevel = request.TargetLevel.HasValue
                ? new List<int> { (int)request.TargetLevel.Value }
                : null,
            Difficulty = MapDifficulty(request.Difficulty),
            AdditionalContext = request.AdditionalContext
        };

        // 2. Call Python AI service
        var aiResponse = await _aiClient.GenerateQuestionsAsync(aiRequest);

        // 3. Map to CreateQuestionDto and save to DB
        var createdQuestions = new List<QuestionDto>();
        foreach (var aiQuestion in aiResponse.Questions)
        {
            var createDto = new CreateQuestionDto
            {
                SectionId = request.SectionId,
                Type = Enum.Parse<QuestionType>(aiQuestion.Type),
                Content = aiQuestion.Content,
                TargetLevel = (ProficiencyLevel)aiQuestion.TargetLevel,
                Difficulty = Enum.Parse<DifficultyLevel>(aiQuestion.Difficulty),
                Points = aiQuestion.Points,
                GradingRubric = aiQuestion.GradingRubric,
                Options = aiQuestion.Options?.Select(o => new CreateQuestionOptionDto
                {
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    DisplayOrder = o.DisplayOrder,
                    Explanation = o.Explanation
                }).ToList() ?? new()
            };

            var created = await _service.CreateQuestionAsync(createDto);
            createdQuestions.Add(created);
        }

        return Ok(createdQuestions);
    }
    catch (HttpRequestException ex)
    {
        return StatusCode(503, $"AI service unavailable: {ex.Message}");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error: {ex.Message}");
    }
}

// Helper methods
private string MapQuestionType(QuestionType type) => type.ToString();
private string? MapDifficulty(DifficultyLevel? diff) => diff?.ToString();
```

---

### Step 3: Frontend (Optional) ⏸️ SKIP

Frontend đã sẵn sàng, không cần update!

---

### Step 4: Testing 🧪 TODO

**4.1. Test Python AI Service**

```bash
curl -X POST http://localhost:8002/api/v2/generate-questions \
  -H "Content-Type: application/json" \
  -d '{
    "question_type": ["Multiple Choice"],
    "language": "English",
    "number_of_questions": 5,
    "skills": [{
      "skill_id": "30000000-0000-0000-0000-000000000078",
      "skill_name": "Accessibility"
    }],
    "target_proficiency_level": [3],
    "difficulty": "Medium"
  }'
```

**4.2. Test C# Backend**

```bash
curl -X POST http://localhost:5000/api/questions/generate-ai \
  -H "Content-Type: application/json" \
  -d '{
    "sectionId": "uuid-here",
    "questionTypes": [1],
    "language": "en",
    "questionCount": 5
  }'
```

**4.3. Test Full Flow via UI**

1. Navigate to Test Template Detail
2. Click "AI Generate" button
3. Fill form and submit
4. Verify questions created

---

## ⚠️ Key Differences to Handle

| Frontend | Python AI | C# Mapping |
|----------|-----------|------------|
| `questionTypes: [1, 2]` (enums) | `question_type: ["Multiple Choice"]` (strings) | Map enum → string |
| `language: "en"` | `language: "English"` | Map "en" → "English" |
| `questionCount` | `number_of_questions` | Rename field |
| `skillId: "uuid"` (single) | `skills: [{...}]` (array) | Wrap in array |
| `targetLevel: 3` (single) | `target_proficiency_level: [3]` (array) | Wrap in array |
| `difficulty: 2` (enum) | `difficulty: "Medium"` (string) | Map enum → string |

---

## 📝 Summary Checklist

### Python AI Service
- [ ] Create `routes_v2.py`
- [ ] Update `main.py` to include router
- [ ] Implement question generator (use Gemini)
- [ ] Test endpoint standalone

### C# Backend
- [ ] Create `AiQuestionGeneratorClient.cs`
- [ ] Register HttpClient in DI
- [ ] Add `appsettings.json` config
- [ ] Update `QuestionsController.GenerateFromAi()`
- [ ] Add mapping methods

### Testing
- [ ] Test Python service alone
- [ ] Test C# → Python call
- [ ] Test full UI → C# → Python → DB flow
- [ ] Verify questions saved correctly

### Deployment
- [ ] Run Python service on port 8002
- [ ] Update production config
- [ ] Monitor error logs

---

## 🔗 Files to Modify

1. `ai-gen/src/api/routes_v2.py` - CREATE
2. `ai-gen/main.py` - UPDATE (add router)
3. `src/SkillMatrix.Infrastructure/Services/AiQuestionGeneratorClient.cs` - CREATE
4. `src/SkillMatrix.Api/Program.cs` - UPDATE (register service)
5. `src/SkillMatrix.Api/appsettings.json` - UPDATE (add AI service URL)
6. `src/SkillMatrix.Api/Controllers/QuestionsController.cs` - UPDATE (call AI service)

---

**Estimated Time**: 3-4 hours
**Difficulty**: Medium
**Dependencies**: Python AI service must run on port 8002
