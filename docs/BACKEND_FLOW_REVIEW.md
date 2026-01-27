# Backend Flow Review - SFIA Assessment System

## Overview
Luồng backend cho hệ thống đánh giá kỹ năng SFIA với 6 bước chính, tích hợp AI để generate câu hỏi, chấm bài và phân tích gap.

---

## Current Architecture

### Database Tables
1. **Skills** (existing)
   - Id (Guid)
   - Code (SFIA code)
   - Name
   - SubcategoryId

2. **Assessment** (existing)
   - Id (Guid)
   - EmployeeId
   - TestTemplateId
   - Status
   - Score, MaxScore, Percentage
   - AiAnalysis, AiRecommendations

3. **AssessmentSkillResult** (existing)
   - AssessmentId
   - SkillId
   - AssessedLevel (ProficiencyLevel enum: 0-7)
   - CorrectAnswers, TotalQuestions
   - ScorePercentage
   - AiExplanation

4. **CourseraCourse** (NEW - just added)
   - SkillId (links to Skills table)
   - Url, Title, Instructor
   - Rating, ReviewsCount
   - Duration, Level, Language
   - Skills[], Syllabus[]
   - Price, CertificateAvailable

### AI Service (Python FastAPI - ai-gen/)
Base URL: `http://localhost:8002/api/v2`

**Endpoints:**
- POST `/generate-questions` - Generate assessment questions
- POST `/grade-answer` - Grade subjective answers
- POST `/analyze-gap` - Analyze single skill gap
- POST `/analyze-gaps` - Analyze multiple skill gaps
- POST `/generate-learning-path` - Generate learning path
- POST `/rank-resources` - Rank learning resources

---

## Implementation Flow

### STEP 1: GENERATE QUESTIONS
**Endpoint:** `POST /api/assessments/generate-questions`

**Input:**
```json
{
  "skillId": "guid",
  "targetProficiencyLevels": [1, 2, 3, 4],
  "questionCount": 10
}
```

**Process:**
1. Get skill info from DB (Code, Name)
2. Get level definitions from `SkillLevelDefinitions` table
3. Call AI Service:
   ```
   POST http://localhost:8002/api/v2/generate-questions
   {
     "skills": [{
       "skill_id": "guid",
       "skill_name": "Strategic planning",
       "skill_code": "ITSP"
     }],
     "target_proficiency_level": [1,2,3,4],
     "number_of_questions": 10,
     "question_type": ["MultipleChoice", "ShortAnswer"],
     "language": "Vietnamese",
     "difficulty": "Medium"
   }
   ```
4. AI returns questions with target_level for each question
5. Save questions to `Question` table (if saving for reuse)
6. Return questions to frontend

**Output:**
```json
{
  "questions": [
    {
      "id": "q_001",
      "type": "MultipleChoice",
      "content": "Question text...",
      "options": [...],
      "proficiency_level": 3,
      "skill_id": "guid",
      "points": 10
    }
  ],
  "metadata": {
    "total_questions": 10,
    "skill_name": "Strategic planning",
    "covers_levels": [1,2,3,4]
  }
}
```

---

### STEP 2: CANDIDATE TAKES ASSESSMENT
**Handled by Frontend/Backend coordination**

**Endpoints:**
- `POST /api/assessments/start` - Create assessment session
- `POST /api/assessments/{id}/submit-answer` - Submit each answer
- `POST /api/assessments/{id}/complete` - Complete assessment

**Data Stored:**
- `Assessment` record (status: InProgress → Completed)
- `AssessmentResponse` records for each question/answer

---

### STEP 3: GRADE SUBJECTIVE ANSWERS
**Endpoint:** `POST /api/assessments/{id}/grade-subjective`

**Process:**
1. Get all ShortAnswer/LongAnswer questions that need grading
2. For each question:
   ```
   POST http://localhost:8002/api/v2/grade-answer
   {
     "question_id": "q_001",
     "question_content": "Explain...",
     "student_answer": "User's answer...",
     "max_points": 10,
     "expected_answer": "Expected answer...",
     "question_type": "ShortAnswer",
     "language": "vi"
   }
   ```
3. AI returns:
   ```json
   {
     "points_awarded": 7,
     "max_points": 10,
     "percentage": 70.0,
     "feedback": "Good explanation...",
     "strength_points": ["Point 1", "Point 2"],
     "improvement_areas": ["Area 1"]
   }
   ```
4. Update `AssessmentResponse` with AI score and feedback
5. Skip MultipleChoice (already has is_correct from options)

---

### STEP 4: EVALUATE ASSESSMENT → CurrentLevel
**Endpoint:** `POST /api/assessments/{id}/evaluate`

**Logic:** Bottom-up consecutive ≥70% rule
```
For skill_id, get all responses:
  - L1 questions: 5/5 correct (100%) ✓
  - L2 questions: 4/5 correct (80%) ✓
  - L3 questions: 2/5 correct (40%) ✗

Result: current_level = 2 (L3 broke the chain)
```

**Process:**
1. Group responses by skill_id and target_level
2. Calculate success rate for each level:
   ```sql
   For each level in [1,2,3,4,5,6,7]:
     total_questions = COUNT(questions where target_level = level)
     correct_answers = SUM(is_correct OR (points_awarded/max_points >= 0.7))
     success_rate = correct_answers / total_questions
   ```
3. Find current_level using bottom-up rule:
   ```csharp
   int currentLevel = 0;
   for (int level = 1; level <= 7; level++) {
     if (successRates[level] >= 0.7) {
       currentLevel = level;
     } else {
       break; // Chain broken
     }
   }
   ```
4. Save to `AssessmentSkillResult`:
   ```json
   {
     "assessmentId": "guid",
     "skillId": "guid",
     "assessedLevel": 2,
     "correctAnswers": 9,
     "totalQuestions": 10,
     "scorePercentage": 90.0
   }
   ```

---

### STEP 5: ANALYZE GAP
**Endpoint:** `POST /api/assessments/{id}/analyze-gaps`

**Input:**
```json
{
  "assessmentId": "guid"
}
```

**Process:**
1. Get Assessment with SkillResults
2. Get Employee's Role requirements from `RoleSkillRequirement`
3. For each skill where current_level < required_level:
   ```
   POST http://localhost:8002/api/v2/analyze-gap
   {
     "employee_name": "Nguyen Van A",
     "job_role": "Senior Backend Developer",
     "skill_id": "guid",
     "skill_name": "System Design",
     "skill_code": "SYSDES",
     "current_level": 2,
     "required_level": 4,
     "language": "vi"
   }
   ```
4. AI returns:
   ```json
   {
     "ai_analysis": "Phân tích chi tiết...",
     "ai_recommendation": "Khuyến nghị cụ thể...",
     "priority_rationale": "Lý do ưu tiên...",
     "estimated_effort": "4-6 tháng",
     "key_actions": ["Action 1", "Action 2"],
     "potential_blockers": ["Blocker 1"]
   }
   ```
5. Update `AssessmentSkillResult` with AI analysis
6. Calculate gap size: `gap_size = required_level - current_level`

**Output:**
```json
{
  "gaps": [
    {
      "skillId": "guid",
      "skillName": "System Design",
      "currentLevel": 2,
      "requiredLevel": 4,
      "gapSize": 2,
      "aiAnalysis": "...",
      "aiRecommendation": "...",
      "estimatedEffort": "4-6 months",
      "keyActions": [...],
      "potentialBlockers": [...]
    }
  ],
  "priorityOrder": ["System Design", "Cloud Architecture"]
}
```

---

### STEP 6: GENERATE LEARNING PATH
**Endpoint:** `POST /api/learning-paths/generate`

**Input:**
```json
{
  "skillId": "guid",
  "currentLevel": 2,
  "targetLevel": 4,
  "timeConstraintMonths": 6
}
```

**Process:**
1. Get skill gap info from Step 5
2. Get available Coursera courses from `CourseraCourse` table:
   ```sql
   SELECT * FROM "CourseraCourse"
   WHERE "SkillId" = @skillId
   AND "Level" IN ('Beginner', 'Intermediate', 'Advanced')
   ORDER BY "Rating" DESC, "ReviewsCount" DESC
   LIMIT 10
   ```
3. Transform courses to resource format:
   ```json
   {
     "id": "course-1",
     "title": "System Design Interview Course",
     "type": "Course",
     "description": "...",
     "estimated_hours": 40,
     "difficulty": "Medium",
     "from_level": 2,
     "to_level": 4
   }
   ```
4. Call AI Service:
   ```
   POST http://localhost:8002/api/v2/generate-learning-path
   {
     "employee_name": "Nguyen Van A",
     "skill_id": "guid",
     "skill_name": "System Design",
     "skill_code": "SYSDES",
     "current_level": 2,
     "target_level": 4,
     "available_resources": [...courses],
     "time_constraint_months": 6,
     "language": "vi"
   }
   ```
5. AI returns structured learning path with milestones
6. Save to `LearningPath` table (if creating new table)
7. Link recommended courses from Coursera

**Output:**
```json
{
  "pathTitle": "Lộ trình phát triển System Design từ Level 2 đến Level 4",
  "pathDescription": "...",
  "estimatedTotalHours": 120,
  "estimatedDurationWeeks": 24,
  "learningItems": [
    {
      "order": 1,
      "title": "Fundamentals of Distributed Systems",
      "itemType": "Course",
      "estimatedHours": 20,
      "targetLevelAfter": 3,
      "resourceId": "course-1",
      "courseUrl": "https://coursera.org/...",
      "successCriteria": "Hoàn thành khóa học và quiz"
    }
  ],
  "milestones": [...]
}
```

---

## Database Schema Changes Needed

### 1. Add LearningPath Table
```sql
CREATE TABLE "LearningPath" (
  "Id" UUID PRIMARY KEY,
  "EmployeeId" UUID NOT NULL,
  "SkillId" UUID NOT NULL,
  "CurrentLevel" INT NOT NULL,
  "TargetLevel" INT NOT NULL,
  "PathTitle" VARCHAR(500),
  "PathDescription" TEXT,
  "EstimatedTotalHours" INT,
  "EstimatedDurationWeeks" INT,
  "Status" VARCHAR(50), -- Draft, Active, Completed
  "CreatedDate" TIMESTAMP,
  "CompletedDate" TIMESTAMP,
  FOREIGN KEY ("EmployeeId") REFERENCES "Employees"("Id"),
  FOREIGN KEY ("SkillId") REFERENCES "Skills"("Id")
);
```

### 2. Add LearningPathItem Table
```sql
CREATE TABLE "LearningPathItem" (
  "Id" UUID PRIMARY KEY,
  "LearningPathId" UUID NOT NULL,
  "Order" INT NOT NULL,
  "Title" VARCHAR(500),
  "Description" TEXT,
  "ItemType" VARCHAR(50), -- Course, Book, Project, etc.
  "EstimatedHours" INT,
  "TargetLevelAfter" INT,
  "Status" VARCHAR(50), -- NotStarted, InProgress, Completed
  "CourseId" INT NULL, -- FK to CourseraCourse if applicable
  "ExternalUrl" TEXT,
  "SuccessCriteria" TEXT,
  "CompletedDate" TIMESTAMP,
  FOREIGN KEY ("LearningPathId") REFERENCES "LearningPath"("Id"),
  FOREIGN KEY ("CourseId") REFERENCES "CourseraCourse"("Id")
);
```

### 3. Update CourseraCourse Foreign Key
```sql
-- Already done! CourseraCourse.SkillId links to Skills table via SFIA mapping
-- Need to create mapping table for SFIA → internal Skills
```

---

## API Controllers Needed

### 1. AssessmentController (extend existing)
```csharp
[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
  // Existing endpoints...

  [HttpPost("generate-questions")]
  public async Task<ActionResult<GenerateQuestionsResponse>> GenerateQuestions(
    GenerateQuestionsRequest request);

  [HttpPost("{id}/grade-subjective")]
  public async Task<ActionResult> GradeSubjectiveAnswers(Guid id);

  [HttpPost("{id}/evaluate")]
  public async Task<ActionResult<EvaluationResult>> EvaluateAssessment(Guid id);

  [HttpPost("{id}/analyze-gaps")]
  public async Task<ActionResult<GapAnalysisResult>> AnalyzeGaps(Guid id);
}
```

### 2. LearningPathsController (NEW)
```csharp
[ApiController]
[Route("api/[controller]")]
public class LearningPathsController : ControllerBase
{
  [HttpPost("generate")]
  public async Task<ActionResult<LearningPathDto>> GenerateLearningPath(
    GenerateLearningPathRequest request);

  [HttpGet("employee/{employeeId}")]
  public async Task<ActionResult<List<LearningPathDto>>> GetEmployeePaths(
    Guid employeeId);

  [HttpPost("{id}/items/{itemId}/complete")]
  public async Task<ActionResult> CompletePathItem(Guid id, Guid itemId);
}
```

### 3. CourseraController (NEW)
```csharp
[ApiController]
[Route("api/[controller]")]
public class CourseraController : ControllerBase
{
  [HttpGet("skill/{skillId}")]
  public async Task<ActionResult<List<CourseraCourseDto>>> GetCoursesBySkill(
    Guid skillId);

  [HttpPost("rank")]
  public async Task<ActionResult<RankedCoursesResponse>> RankCourses(
    RankCoursesRequest request);
}
```

---

## Service Layer Implementation

### 1. AiQuestionGeneratorService (existing - already implemented)
- `GenerateQuestionsAsync()`
- Uses GeminiAiQuestionGeneratorService or AzureOpenAI

### 2. AiGradingService (NEW)
```csharp
public interface IAiGradingService
{
  Task<GradingResult> GradeAnswerAsync(GradeAnswerRequest request);
}
```

### 3. AssessmentEvaluationService (NEW)
```csharp
public interface IAssessmentEvaluationService
{
  Task<EvaluationResult> EvaluateAssessmentAsync(Guid assessmentId);
  Task<int> CalculateCurrentLevelAsync(Guid skillId, List<AssessmentResponse> responses);
}
```

### 4. GapAnalysisService (NEW)
```csharp
public interface IGapAnalysisService
{
  Task<GapAnalysisResult> AnalyzeGapsAsync(Guid assessmentId);
  Task<SkillGapDto> AnalyzeSingleGapAsync(SkillGapInput input);
}
```

### 5. LearningPathService (NEW)
```csharp
public interface ILearningPathService
{
  Task<LearningPathDto> GeneratePathAsync(GenerateLearningPathRequest request);
  Task<List<CourseraCourseDto>> GetRecommendedCoursesAsync(Guid skillId, int fromLevel, int toLevel);
}
```

---

## Configuration

### appsettings.json
```json
{
  "AiService": {
    "BaseUrl": "http://localhost:8002/api/v2",
    "Timeout": 120,
    "RetryCount": 3
  },
  "Assessment": {
    "PassingThreshold": 0.7,
    "MaxQuestionsPerLevel": 5,
    "DefaultLanguage": "Vietnamese"
  }
}
```

---

## Testing Strategy

### Unit Tests
1. AssessmentEvaluationService
   - Test bottom-up consecutive rule
   - Test edge cases (all pass, all fail, partial)
2. GapAnalysisService
   - Mock AI responses
   - Test gap calculation

### Integration Tests
1. End-to-end assessment flow
2. AI service integration tests
3. Database persistence tests

---

## Next Steps

### Phase 1: Core Assessment (Priority 1)
- [ ] Implement Step 1: Generate Questions
- [ ] Implement Step 2: Take Assessment (frontend + backend)
- [ ] Implement Step 3: Grade Subjective
- [ ] Implement Step 4: Evaluate Assessment

### Phase 2: Gap Analysis (Priority 2)
- [ ] Implement Step 5: Analyze Gap
- [ ] Create gap analysis UI
- [ ] Test with real AI service

### Phase 3: Learning Path (Priority 3)
- [ ] Create LearningPath tables
- [ ] Implement Step 6: Generate Learning Path
- [ ] Link with Coursera courses
- [ ] Create learning path tracking UI

### Phase 4: Integration & Polish
- [ ] End-to-end testing
- [ ] Performance optimization
- [ ] Error handling & retry logic
- [ ] Logging & monitoring

---

## Notes

### SFIA Skill ID Mapping
- CourseraCourse table uses SFIA skill_id (UUID from ai-gen DB)
- Need to map to internal Skills table
- Options:
  1. Update Skills table to store SFIA_skill_id
  2. Create mapping table
  3. Match by skill_code (most reliable)

### AI Service Reliability
- Add circuit breaker pattern
- Cache AI responses for common queries
- Fallback to rule-based evaluation if AI fails

### Scalability
- Question generation can be slow (10-30s)
- Consider background job processing
- Cache generated questions for reuse
