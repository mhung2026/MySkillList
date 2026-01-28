# Implementation Checklist - AI Integration Steps 3-6

## Overview
This checklist tracks the implementation progress for the remaining AI integration steps.

**Current Status:**
- ✅ Step 1: Generate Questions - COMPLETED
- ✅ Step 2: Take Assessment - COMPLETED
- ⏳ Step 3-6: TO BE IMPLEMENTED

---

## Step 3: Grade Subjective Answers

### Backend Tasks
- [ ] Create DTOs
  - [ ] `GradeAnswerRequest` in `src/SkillMatrix.Application/DTOs/Assessment/AiGradingDto.cs`
  - [ ] `GradingResult` in same file
- [ ] Create Service
  - [ ] Interface `IAiGradingService`
  - [ ] Implementation `GeminiAiGradingService`
  - [ ] Register in DI container
- [ ] Add Controller Endpoint
  - [ ] `POST /api/assessments/{id}/grade-subjective` in `AssessmentsController.cs`
- [ ] Database Migration
  - [ ] Add `AiFeedback` column to `AssessmentResponse`
  - [ ] Add `FeedbackDetails` column
  - [ ] Add `IsGraded` column
  - [ ] Create index for ungraded responses

### Frontend Tasks
- [ ] Create API client
  - [ ] Add `gradeSubjectiveAnswers()` to `web/src/api/assessments.ts`
- [ ] Create UI Component
  - [ ] Grade button in `AssessmentResults.tsx`
  - [ ] Display AI feedback
  - [ ] Show grading progress

### Testing
- [ ] Unit test for `GeminiAiGradingService`
- [ ] Integration test for grading endpoint
- [ ] Manual test with real assessment

**Estimated Time:** 4-6 hours

---

## Step 4: Evaluate Assessment

### Backend Tasks
- [ ] Create DTOs
  - [ ] `EvaluationResult` in `src/SkillMatrix.Application/DTOs/Assessment/EvaluationDto.cs`
  - [ ] `SkillEvaluation`
- [ ] Create Service
  - [ ] Interface `IAssessmentEvaluationService`
  - [ ] Implementation with bottom-up consecutive ≥70% logic
  - [ ] `CalculateCurrentLevelAsync()` method
  - [ ] Register in DI
- [ ] Add Controller Endpoint
  - [ ] `POST /api/assessments/{id}/evaluate`
- [ ] Update Domain Models
  - [ ] Ensure `AssessmentSkillResult` has all needed fields

### Frontend Tasks
- [ ] Create API client
  - [ ] Add `evaluateAssessment()` to API
- [ ] Create UI Component
  - [ ] Evaluation button
  - [ ] Display skill evaluations
  - [ ] Show level success rates
  - [ ] Visual progress indicators

### Testing
- [ ] Unit test for bottom-up logic
  - [ ] Test case: L1=100%, L2=80%, L3=40% → result=2
  - [ ] Test case: All levels pass → result=highest level
  - [ ] Test case: First level fails → result=0
- [ ] Integration test for evaluation endpoint
- [ ] Manual test with real assessment data

**Estimated Time:** 6-8 hours

---

## Step 5: Analyze Gap

### Backend Tasks
- [ ] Create DTOs
  - [ ] `AnalyzeGapRequest` in `src/SkillMatrix.Application/DTOs/Assessment/GapAnalysisDto.cs`
  - [ ] `GapAnalysisResult`
  - [ ] `GapAnalysisResponse`
  - [ ] `SkillGapDto`
- [ ] Create Service
  - [ ] Interface `IGapAnalysisService`
  - [ ] Implementation `GeminiGapAnalysisService`
  - [ ] Register in DI
- [ ] Add Controller Endpoint
  - [ ] `POST /api/assessments/{id}/analyze-gaps`
  - [ ] Get role requirements
  - [ ] Find gaps (required > current)
  - [ ] Call AI for each gap
  - [ ] Save analysis to database

### Frontend Tasks
- [ ] Create API client
  - [ ] Add `analyzeGaps()` to API
- [ ] Create UI Component
  - [ ] `GapAnalysis.tsx` page
  - [ ] Display gap cards
  - [ ] Show AI analysis
  - [ ] Show recommendations
  - [ ] List key actions
  - [ ] List potential blockers
  - [ ] Priority ordering

### Testing
- [ ] Unit test for gap analysis service
- [ ] Integration test with mock AI responses
- [ ] Manual test with real skills and levels

**Estimated Time:** 8-10 hours

---

## Step 6: Generate Learning Path

### Backend Tasks
- [ ] Create DTOs
  - [ ] `GenerateLearningPathRequest` in `src/SkillMatrix.Application/DTOs/LearningPath/LearningPathDto.cs`
  - [ ] `LearningPathResponse`
  - [ ] `LearningResourceInfo`
  - [ ] `LearningItem`
  - [ ] `Milestone`
  - [ ] `CourseraCourseDto`
- [ ] Create Repository
  - [ ] Interface `ICourseraCourseRepository`
  - [ ] Implementation with Dapper queries
  - [ ] Query Coursera courses by skill code
- [ ] Create Service
  - [ ] Interface `ILearningPathService`
  - [ ] Implementation `LearningPathService`
  - [ ] Get Coursera courses from DB
  - [ ] Transform to AI format
  - [ ] Call AI service
  - [ ] Parse duration strings
  - [ ] Map difficulty levels
  - [ ] Register in DI
- [ ] Create Controller
  - [ ] `LearningPathsController.cs`
  - [ ] `POST /learning-paths/generate`
  - [ ] `GET /learning-paths/skill/{skillCode}/courses`
- [ ] Database Migration
  - [ ] Create `LearningPath` table
  - [ ] Create `LearningPathItem` table
  - [ ] Create `LearningPathMilestone` table
  - [ ] Create indexes

### Frontend Tasks
- [ ] Create API client
  - [ ] `web/src/api/learningPath.ts`
  - [ ] `generatePath()` function
  - [ ] `getCoursesBySkill()` function
- [ ] Create UI Components
  - [ ] `LearningPathGenerator.tsx` main component
  - [ ] Generate button with loading state
  - [ ] Display path overview (hours, weeks, items)
  - [ ] Timeline of learning items
  - [ ] Item cards with details
  - [ ] Milestone indicators
  - [ ] AI insights display
  - [ ] Success factors list
  - [ ] Challenges list

### Testing
- [ ] Unit test for Coursera repository
- [ ] Unit test for learning path service
- [ ] Unit test for duration parsing
- [ ] Integration test for path generation
- [ ] Manual test with real Coursera data

**Estimated Time:** 10-12 hours

---

## Database Migrations Summary

### Migration 1: Add AI Grading Fields
```sql
-- File: 20260127_AddAiGradingFields.sql
ALTER TABLE "AssessmentResponse"
ADD COLUMN "AiFeedback" TEXT,
ADD COLUMN "FeedbackDetails" TEXT,
ADD COLUMN "IsGraded" BOOLEAN DEFAULT FALSE;

CREATE INDEX idx_assessment_response_grading
ON "AssessmentResponse"("AssessmentId", "IsGraded")
WHERE "IsGraded" = FALSE;
```

### Migration 2: Add Learning Path Tables
```sql
-- File: 20260127_AddLearningPathTables.sql
-- See full migration in AI_BACKEND_FRONTEND_IMPLEMENTATION.md
-- Creates: LearningPath, LearningPathItem, LearningPathMilestone
```

---

## Configuration Updates

### appsettings.json
```json
{
  "AiService": {
    "BaseUrl": "http://localhost:8002",
    "TimeoutSeconds": 120,
    "RetryCount": 3
  }
}
```

### appsettings.Production.json
```json
{
  "AiService": {
    "BaseUrl": "https://myskilllist-ngeteam-ai.allianceitsc.com"
  }
}
```

---

## DI Registration Checklist

**File:** `src/SkillMatrix.Api/Program.cs`

```csharp
// Step 3: AI Grading
builder.Services.AddHttpClient<IAiGradingService, GeminiAiGradingService>();

// Step 4: Evaluation
builder.Services.AddScoped<IAssessmentEvaluationService, AssessmentEvaluationService>();

// Step 5: Gap Analysis
builder.Services.AddHttpClient<IGapAnalysisService, GeminiGapAnalysisService>();

// Step 6: Learning Path
builder.Services.AddHttpClient<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<ICourseraCourseRepository, CourseraCourseRepository>();
```

---

## Testing Checklist

### Unit Tests
- [ ] `AssessmentEvaluationServiceTests.cs`
  - [ ] Test bottom-up consecutive logic
  - [ ] Test edge cases
- [ ] `GapAnalysisServiceTests.cs`
  - [ ] Test AI integration
  - [ ] Test error handling
- [ ] `LearningPathServiceTests.cs`
  - [ ] Test Coursera integration
  - [ ] Test duration parsing

### Integration Tests
- [ ] Grade subjective answers end-to-end
- [ ] Evaluate assessment with real data
- [ ] Analyze gaps with role requirements
- [ ] Generate learning path with Coursera courses

### Manual Testing
- [ ] Create test assessment with subjective questions
- [ ] Submit answers and grade
- [ ] Evaluate and verify level calculation
- [ ] Analyze gaps for test employee
- [ ] Generate learning path and verify Coursera courses

---

## Implementation Order (Recommended)

### Week 1: Infrastructure & Step 3
**Day 1-2:**
- [ ] Run database migrations
- [ ] Set up DI registrations
- [ ] Create base DTOs

**Day 3-5:**
- [ ] Implement Step 3 (Grading)
- [ ] Write tests
- [ ] Manual testing

### Week 2: Step 4 & 5
**Day 1-3:**
- [ ] Implement Step 4 (Evaluation)
- [ ] Test bottom-up logic thoroughly
- [ ] Manual testing with various scenarios

**Day 4-5:**
- [ ] Implement Step 5 (Gap Analysis)
- [ ] Integrate with role requirements
- [ ] Test with real skills

### Week 3: Step 6 & Integration
**Day 1-4:**
- [ ] Implement Step 6 (Learning Path)
- [ ] Set up Coursera repository
- [ ] Test with real Coursera data
- [ ] Create frontend components

**Day 5:**
- [ ] End-to-end integration testing
- [ ] Performance testing
- [ ] Bug fixes

### Week 4: Polish & Deploy
**Day 1-2:**
- [ ] UI/UX improvements
- [ ] Error handling
- [ ] Loading states

**Day 3-4:**
- [ ] Code review
- [ ] Documentation updates
- [ ] Performance optimization

**Day 5:**
- [ ] Production deployment
- [ ] Smoke testing
- [ ] User training

---

## Success Criteria

### Technical
- ✅ All 6 steps implemented and working
- ✅ Unit tests pass with >80% coverage
- ✅ Integration tests pass
- ✅ AI service integration stable
- ✅ No breaking changes to existing code

### Functional
- ✅ Users can generate questions with AI
- ✅ Subjective answers graded automatically
- ✅ Current levels calculated correctly
- ✅ Gap analysis provides useful insights
- ✅ Learning paths include relevant Coursera courses

### Performance
- ✅ Question generation: <30 seconds
- ✅ Grading: <10 seconds per question
- ✅ Evaluation: <5 seconds
- ✅ Gap analysis: <10 seconds per gap
- ✅ Learning path: <20 seconds

---

## Risk Mitigation

### Risk 1: AI Service Timeout
**Mitigation:**
- Increase timeout to 120 seconds
- Implement retry logic
- Add circuit breaker pattern

### Risk 2: Data Mapping Errors
**Mitigation:**
- Comprehensive unit tests
- Integration tests with real data
- Proper error handling and logging

### Risk 3: Coursera Data Quality
**Mitigation:**
- Data validation on insert
- Handle missing fields gracefully
- Provide fallback options

---

## Documentation Updates

- [ ] Update README.md with new features
- [ ] Update API documentation
- [ ] Create user guide for learning paths
- [ ] Update deployment guide
- [ ] Add troubleshooting section

---

**Total Estimated Time:** 4-5 weeks (1 developer)

**Status:** ⏳ READY TO START

**Last Updated:** 2026-01-27
