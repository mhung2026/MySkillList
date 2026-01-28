# 📋 AI Integration - Quick Start Guide

## 🎯 What We Have

### ✅ Completed (Already Working)
1. **AI Service** running on Python FastAPI
   - Base URL: `http://localhost:8002/api/v2`
   - Production: `https://myskilllist-ngeteam-ai.allianceitsc.com/api/v2`
   - Connected to PostgreSQL with 146 SFIA skills

2. **Step 1: Generate Questions** ✅
   - Backend: `POST /api/questions/generate-ai`
   - Uses Azure OpenAI GPT-4o
   - Tested and working

3. **Step 2: Take Assessment** ✅
   - Frontend quiz interface
   - Response collection
   - Data persistence

4. **Coursera Data** ✅
   - 146 SFIA skills
   - 409 Coursera courses
   - Stored in `CourseraCourse` table

---

## 🚀 What to Implement Next

### Step 3: Grade Subjective Answers (4-6 hours)
**Goal:** Automatically grade ShortAnswer and LongAnswer questions using AI

**Key Files:**
- `src/SkillMatrix.Application/DTOs/Assessment/AiGradingDto.cs` (NEW)
- `src/SkillMatrix.Application/Services/AI/GeminiAiGradingService.cs` (NEW)
- `src/SkillMatrix.Api/Controllers/AssessmentsController.cs` (EXTEND)
- `web/src/api/assessments.ts` (EXTEND)

**API Endpoint:**
```
POST /api/assessments/{id}/grade-subjective
```

**AI Endpoint Used:**
```
POST /api/v2/grade-answer
```

**What It Does:**
1. Gets all ungraded subjective responses
2. Sends each to AI service for grading
3. Receives score + feedback
4. Updates response with `PointsAwarded`, `AiFeedback`, `IsGraded`

---

### Step 4: Evaluate Assessment (6-8 hours)
**Goal:** Calculate current skill level using bottom-up consecutive ≥70% rule

**Key Files:**
- `src/SkillMatrix.Application/Services/Assessment/AssessmentEvaluationService.cs` (NEW)
- `src/SkillMatrix.Api/Controllers/AssessmentsController.cs` (EXTEND)
- `web/src/pages/assessments/EvaluationResults.tsx` (NEW)

**API Endpoint:**
```
POST /api/assessments/{id}/evaluate
```

**Algorithm:**
```
For each skill:
  L1: 5/5 correct (100%) ✓ → current_level = 1
  L2: 4/5 correct (80%) ✓ → current_level = 2
  L3: 2/5 correct (40%) ✗ → STOP (chain broken)

Result: current_level = 2
```

**What It Does:**
1. Groups responses by skill and level
2. Calculates success rate for each level
3. Applies bottom-up consecutive rule
4. Saves `AssessedLevel` to `AssessmentSkillResult`

---

### Step 5: Analyze Gap (8-10 hours)
**Goal:** Get AI analysis and recommendations for skill gaps

**Key Files:**
- `src/SkillMatrix.Application/Services/AI/GeminiGapAnalysisService.cs` (NEW)
- `src/SkillMatrix.Api/Controllers/AssessmentsController.cs` (EXTEND)
- `web/src/pages/assessments/GapAnalysis.tsx` (NEW)

**API Endpoint:**
```
POST /api/assessments/{id}/analyze-gaps
```

**AI Endpoint Used:**
```
POST /api/v2/analyze-gap
```

**What It Does:**
1. Gets employee's role requirements
2. Compares `current_level` vs `required_level`
3. For each gap, calls AI for analysis
4. Receives:
   - AI analysis of the gap
   - Recommendations
   - Estimated effort (e.g., "4-6 months")
   - Key actions to take
   - Potential blockers

---

### Step 6: Generate Learning Path (10-12 hours)
**Goal:** Create personalized learning roadmap with Coursera courses

**Key Files:**
- `src/SkillMatrix.Application/Services/LearningPath/LearningPathService.cs` (NEW)
- `src/SkillMatrix.Infrastructure/Repositories/CourseraCourseRepository.cs` (NEW)
- `src/SkillMatrix.Api/Controllers/LearningPathsController.cs` (NEW)
- `web/src/pages/learningPath/LearningPathGenerator.tsx` (NEW)

**API Endpoints:**
```
POST /api/learning-paths/generate
GET /api/learning-paths/skill/{skillCode}/courses
```

**AI Endpoint Used:**
```
POST /api/v2/generate-learning-path
```

**What It Does:**
1. Gets Coursera courses from database (by skill_code)
2. Transforms to AI format
3. Calls AI to generate structured learning path
4. Receives:
   - Path title and description
   - Learning items (courses, books, projects)
   - Milestones
   - Estimated hours and duration
   - Success factors
   - Potential challenges

---

## 📂 Document Structure

```
docs/
├── BACKEND_FLOW_REVIEW.md           # Overall architecture and flow
├── AI_BACKEND_FRONTEND_IMPLEMENTATION.md  # Complete code implementation
├── IMPLEMENTATION_CHECKLIST.md       # Step-by-step checklist
└── AI_INTEGRATION_QUICKSTART.md     # This file (summary)

ai-gen/
├── API_DOCUMENTATION.md             # AI service API reference
└── insert_sfia_coursera_data.py     # Script to populate Coursera data
```

---

## 🗄️ Database Changes Needed

### Migration 1: AI Grading Fields
```sql
ALTER TABLE "AssessmentResponse"
ADD COLUMN "AiFeedback" TEXT,
ADD COLUMN "FeedbackDetails" TEXT,
ADD COLUMN "IsGraded" BOOLEAN DEFAULT FALSE;
```

### Migration 2: Learning Path Tables
```sql
CREATE TABLE "LearningPath" (...);
CREATE TABLE "LearningPathItem" (...);
CREATE TABLE "LearningPathMilestone" (...);
```

See full SQL in: `docs/AI_BACKEND_FRONTEND_IMPLEMENTATION.md`

---

## 🔧 Implementation Steps

### Phase 1: Setup (1 hour)
```bash
# 1. Run database migrations
dotnet ef migrations add AddAiGradingFields
dotnet ef database update

# 2. Verify AI service is running
curl http://localhost:8002/api/v2/health

# 3. Check Coursera data
psql -d MySkillList_NGE_DEV -c 'SELECT COUNT(*) FROM "CourseraCourse";'
# Should return: 409
```

### Phase 2: Implement Step 3 (4-6 hours)
1. Create `AiGradingDto.cs`
2. Create `GeminiAiGradingService.cs`
3. Add endpoint to `AssessmentsController.cs`
4. Test with Postman/curl
5. Create frontend UI

### Phase 3: Implement Step 4 (6-8 hours)
1. Create `AssessmentEvaluationService.cs`
2. Implement bottom-up algorithm
3. Write unit tests for edge cases
4. Add endpoint to controller
5. Create frontend results UI

### Phase 4: Implement Step 5 (8-10 hours)
1. Create `GeminiGapAnalysisService.cs`
2. Integrate with role requirements
3. Add endpoint to controller
4. Create frontend gap analysis UI
5. Test with real employee data

### Phase 5: Implement Step 6 (10-12 hours)
1. Create `CourseraCourseRepository.cs`
2. Create `LearningPathService.cs`
3. Create `LearningPathsController.cs`
4. Run learning path migration
5. Create frontend generator UI
6. Test with Coursera data

---

## 🧪 Testing Strategy

### Quick Manual Test Flow

1. **Create Test Assessment**
   - Use test template with mix of question types
   - Include MultipleChoice, ShortAnswer, LongAnswer
   - Cover levels 1-4

2. **Take Assessment**
   - Submit answers
   - Include some subjective answers

3. **Test Step 3: Grade**
   ```bash
   POST /api/assessments/{id}/grade-subjective
   ```
   - Verify AI feedback is saved
   - Check `IsGraded = true`

4. **Test Step 4: Evaluate**
   ```bash
   POST /api/assessments/{id}/evaluate
   ```
   - Verify level calculation is correct
   - Check `AssessedLevel` in database

5. **Test Step 5: Gap Analysis**
   ```bash
   POST /api/assessments/{id}/analyze-gaps
   ```
   - Verify AI analysis is returned
   - Check recommendations are useful

6. **Test Step 6: Learning Path**
   ```bash
   POST /api/learning-paths/generate
   {
     "skillCode": "PROG",
     "currentLevel": 2,
     "targetLevel": 4
   }
   ```
   - Verify Coursera courses are included
   - Check learning items are ordered
   - Verify milestones are present

---

## 📊 Expected Results

### Step 3 Output Example
```json
{
  "gradedCount": 3,
  "results": [
    {
      "questionId": "guid",
      "pointsAwarded": 7,
      "feedback": "Good explanation. Consider adding examples."
    }
  ]
}
```

### Step 4 Output Example
```json
{
  "assessmentId": "guid",
  "skillEvaluations": [
    {
      "skillName": "Programming",
      "assessedLevel": 2,
      "correctAnswers": 9,
      "totalQuestions": 10,
      "scorePercentage": 90.0,
      "levelSuccessRates": {
        "1": 1.0,
        "2": 0.8,
        "3": 0.4
      }
    }
  ]
}
```

### Step 5 Output Example
```json
{
  "gaps": [
    {
      "skillName": "System Design",
      "currentLevel": 2,
      "requiredLevel": 4,
      "gapSize": 2,
      "aiAnalysis": "Khoảng cách 2 cấp độ...",
      "aiRecommendation": "Nên bắt đầu với...",
      "estimatedEffort": "4-6 tháng",
      "keyActions": ["Complete course X", "Practice Y"],
      "potentialBlockers": ["Time constraints"]
    }
  ]
}
```

### Step 6 Output Example
```json
{
  "pathTitle": "Lộ trình phát triển System Design từ Level 2 đến Level 4",
  "estimatedTotalHours": 120,
  "estimatedDurationWeeks": 24,
  "learningItems": [
    {
      "order": 1,
      "title": "System Design Interview Course",
      "itemType": "Course",
      "estimatedHours": 40,
      "targetLevelAfter": 3,
      "successCriteria": "Complete course and pass quiz"
    }
  ],
  "milestones": [
    {
      "afterItem": 2,
      "description": "Can design medium-complexity systems",
      "expectedLevel": 3
    }
  ]
}
```

---

## 🐛 Common Issues & Solutions

### Issue 1: AI Service Timeout
**Error:** `HttpRequestException: The operation was canceled`

**Solution:**
```csharp
// Increase timeout in Program.cs
builder.Services.AddHttpClient<IAiService>(client => {
    client.Timeout = TimeSpan.FromSeconds(120);
});
```

### Issue 2: Coursera Courses Not Found
**Error:** `No courses found for skill code`

**Solution:**
```sql
-- Check skill code exists
SELECT * FROM "SFIASkillCoursera" WHERE "SkillCode" = 'YOUR_CODE';

-- If no courses, check skill_id mapping
SELECT s."SkillCode", COUNT(c."Id")
FROM "SFIASkillCoursera" s
LEFT JOIN "CourseraCourse" c ON s."SkillId" = c."SkillId"
GROUP BY s."SkillCode";
```

### Issue 3: Bottom-up Evaluation Wrong
**Error:** Level calculated incorrectly

**Debug:**
```csharp
// Add logging to see success rates
foreach (var level in successRates) {
    _logger.LogDebug("Level {Level}: {Rate:P}", level.Key, level.Value);
}
```

---

## 📈 Performance Expectations

| Operation | Expected Time | Notes |
|-----------|--------------|-------|
| Generate Questions | 10-30s | Depends on question count |
| Grade 1 Answer | 2-5s | Per subjective question |
| Evaluate Assessment | <5s | All skills at once |
| Analyze 1 Gap | 3-5s | Per skill gap |
| Generate Learning Path | 15-20s | With 5-10 courses |

---

## 🎯 Success Criteria

### Technical
- ✅ All services respond within timeout
- ✅ AI responses are well-formatted
- ✅ Database updates are correct
- ✅ Error handling works
- ✅ Logs are informative

### Functional
- ✅ Grading results are reasonable
- ✅ Level calculation follows rules
- ✅ Gap analysis is insightful
- ✅ Learning paths are actionable
- ✅ Coursera courses are relevant

### User Experience
- ✅ Loading states are clear
- ✅ Error messages are helpful
- ✅ Results are easy to understand
- ✅ Navigation is intuitive

---

## 📞 Support & Resources

**Documentation:**
- Main Docs: `docs/AI_BACKEND_FRONTEND_IMPLEMENTATION.md`
- API Reference: `ai-gen/API_DOCUMENTATION.md`
- Checklist: `docs/IMPLEMENTATION_CHECKLIST.md`

**AI Service:**
- Swagger UI: `http://localhost:8002/api/docs`
- Health Check: `http://localhost:8002/api/v2/health`

**Database:**
- Connection: `192.168.0.21:5432/MySkillList_NGE_DEV`

---

**Ready to Start? Begin with Step 3!** 🚀

**Estimated Total Time:** 4-5 weeks (1 developer)

**Last Updated:** 2026-01-27
