# SRS - SkillMatrix System

## Project Overview

**SkillMatrix** is an employee skill management system for competency assessment and learning path recommendations. The system integrates SFIA 9 framework and supports AI for automatic question generation and skill gap analysis.

---

## DEVELOPMENT RULES

### Language Rule
- **English Only**: All code, comments, documentation, UI text, database content, API responses, and error messages MUST be in English.
- No bilingual/multilingual content needed.
- All enum values, names, descriptions use English only.

---

## 1. DOMAIN ENTITIES

### 1.1 Taxonomy (Skill Classification)

| Entity | Description | Status |
|--------|-------------|--------|
| **SkillDomain** | High-level skill groups (DEV, STRA, PEOP...) | ✅ Done |
| **SkillSubcategory** | Sub-groups within domain | ✅ Done |
| **Skill** | Specific skill with levels | ✅ Done |
| **SkillLevelDefinition** | Behavioral criteria for each skill level | ✅ Done |
| **SkillRelationship** | Relationships between skills (prerequisite, related) | Entity exists, UI pending |
| **ProficiencyLevelDefinition** | SFIA 9 level definitions (1-7) | ✅ Done |

### 1.2 Organization

| Entity | Description | Status |
|--------|-------------|--------|
| **Team** | Department/team (supports hierarchy) | Entity exists, API incomplete |
| **JobRole** | Job positions (BE, FE, QA, BA, PM...) | Entity exists, API pending |
| **RoleSkillRequirement** | Skill requirements per role | Entity exists, API pending |
| **Employee** | Employee with full profile | ✅ Done |
| **EmployeeSkill** | Employee's current skills | ✅ Done |
| **EmployeeSkillHistory** | Skill level change history | ✅ Done |

### 1.3 Project

| Entity | Description | Status |
|--------|-------------|--------|
| **Project** | Project with skill requirements | Entity exists, API pending |
| **ProjectSkillRequirement** | Skills needed for project | Entity exists, API pending |
| **ProjectAssignment** | Employee project assignments | Entity exists, API pending |

### 1.4 Assessment

| Entity | Description | Status |
|--------|-------------|--------|
| **TestTemplate** | Reusable test templates | ✅ Done |
| **TestSection** | Sections within test | ✅ Done |
| **Question** | Questions with multiple types (9 types) | ✅ Done |
| **QuestionOption** | Options for multiple choice/answer/true-false | ✅ Done |
| **Assessment** | Assessment session | ✅ Done |
| **AssessmentSkillResult** | Results per skill | ✅ Done |
| **AssessmentResponse** | Employee responses (text, code, options, rating) | ✅ Done |

### 1.5 Learning

| Entity | Description | Status |
|--------|-------------|--------|
| **LearningResource** | Learning materials (Course, Book, Cert...) | Entity exists, API pending |
| **LearningResourceSkill** | Skills developed by resource | Entity exists, API pending |
| **EmployeeLearningPath** | Personal learning path (AI-generated) | Entity exists, API pending |
| **LearningPathItem** | Items in learning path | Entity exists, API pending |
| **SkillGap** | Skill development gaps | Entity exists, API pending |
| **TeamSkillGap** | Team-level skill gaps | Entity exists, API pending |

### 1.6 Configuration (Dynamic Enums)

| Entity | Description | Status |
|--------|-------------|--------|
| **SystemEnumValue** | Configurable enum values from Admin | ✅ Done |

---

## 2. API ENDPOINTS

### 2.1 Taxonomy Management

```
GET/POST/PUT/DELETE /api/skills                    - CRUD Skills
GET/POST/PUT/DELETE /api/skilldomains              - CRUD Domains
GET/POST/PUT/DELETE /api/skillsubcategories        - CRUD Subcategories
GET/POST/PUT/DELETE /api/leveldefinitions          - CRUD Level Definitions
POST /api/leveldefinitions/seed                    - Seed SFIA 9 defaults
GET /api/enums/*                                   - Get all enums
```

### 2.2 Assessment & Testing

```
# Test Templates
GET    /api/testtemplates                          - List templates
POST   /api/testtemplates                          - Create template
GET    /api/testtemplates/{id}                     - Get template detail
PUT    /api/testtemplates/{id}                     - Update template
DELETE /api/testtemplates/{id}                     - Delete template

# Sections
POST   /api/testtemplates/sections                 - Create section
PUT    /api/testtemplates/sections/{id}            - Update section
DELETE /api/testtemplates/sections/{id}            - Delete section

# Questions (Full CRUD)
GET    /api/questions                              - List questions with filter
GET    /api/questions/{id}                         - Get question detail
POST   /api/questions                              - Create question
PUT    /api/questions/{id}                         - Update question
DELETE /api/questions/{id}                         - Delete question (soft)
POST   /api/questions/{id}/toggle-active           - Toggle active status
POST   /api/questions/bulk                         - Bulk create questions
POST   /api/questions/generate-ai                  - AI question generation

# Assessment Workflow
GET    /api/assessments/available/{employeeId}     - Available tests
POST   /api/assessments/start                      - Start assessment
GET    /api/assessments/{id}/continue              - Continue test
POST   /api/assessments/answer                     - Submit answer
POST   /api/assessments/{id}/submit                - Complete test
GET    /api/assessments/{id}/result                - Get result
```

### 2.3 Authentication

```
POST /api/auth/login                               - Login
POST /api/auth/register                            - Register
GET  /api/auth/me/{userId}                         - Get current user
POST /api/auth/change-password/{userId}            - Change password
GET  /api/auth/users                               - List users (admin)
POST /api/auth/seed                                - Seed default users
```

### 2.4 Dashboard

```
GET /api/dashboard/overview                        - Statistics
GET /api/dashboard/employees/skills                - All employees skills
GET /api/dashboard/employees/{id}/skills           - Single employee
GET /api/dashboard/skill-matrix                    - Team skill matrix
```

### 2.5 AI Services

```
POST /api/ai/generate-questions                    - Generate questions
POST /api/ai/grade-answer                          - Grade answer
POST /api/ai/analyze-skill-gaps                    - Analyze gaps
```

### 2.6 Configuration (Admin)

```
GET    /api/systemenums/types                      - Get all enum types
GET    /api/systemenums/values/{enumType}          - Get values for enum type
GET    /api/systemenums/dropdown/{enumType}        - Get dropdown values
GET    /api/systemenums/{id}                       - Get single value
POST   /api/systemenums                            - Create enum value
PUT    /api/systemenums/{id}                       - Update enum value
DELETE /api/systemenums/{id}                       - Delete enum value
PATCH  /api/systemenums/{id}/toggle-active         - Toggle active
POST   /api/systemenums/reorder                    - Reorder values
POST   /api/systemenums/seed                       - Seed default values
```

### 2.7 Pending APIs (To be developed)

```
/api/jobroles                                      - Job Role management
/api/roleskillrequirements                         - Role skill requirements
/api/teams                                         - Team management
/api/projects                                      - Project management
/api/learningresources                             - Learning resources
/api/learningpaths                                 - Learning paths
/api/skillgaps                                     - Skill gap analysis
```

---

## 3. FRONTEND PAGES

### 3.1 Completed

| Page | Path | Description |
|------|------|-------------|
| Login | `/login` | User login |
| Dashboard | `/dashboard` | Overview, skill matrix |
| Available Tests | `/tests` | Test list for employee |
| Take Test | `/tests/:id/take` | Take a test (supports all 9 question types) |
| Test Result | `/tests/:id/result` | Test results |
| Skill Domains | `/taxonomy/domains` | Manage domains |
| Subcategories | `/taxonomy/subcategories` | Manage subcategories |
| Skills | `/taxonomy/skills` | Manage skills |
| Level Definitions | `/taxonomy/levels` | Manage SFIA levels |
| Skill Levels | `/taxonomy/skill-levels` | Manage skill level definitions |
| Test Templates | `/templates` | Manage templates |
| Template Detail | `/templates/:id` | Template details with Question CRUD |
| System Enums | `/admin/enums` | Manage dynamic enums |

### 3.2 Pending Development

| Page | Description | Priority |
|------|-------------|----------|
| Job Roles | Job position management | HIGH |
| Role Requirements | Skill requirements per role | HIGH |
| Team Management | Team/department management | MEDIUM |
| Learning Resources | Learning material management | MEDIUM |
| Learning Paths | Personal learning paths | MEDIUM |
| Employee Profile | Detailed employee profile | HIGH |
| Skill Gap Report | Skill gap reports | HIGH |
| Admin Dashboard | System administration | MEDIUM |

---

## 4. DYNAMIC ENUMERATIONS (Configurable)

### 4.1 Enum Types (Admin Configurable)

These enums are stored in database and configurable from Admin:

| Enum Type | Description | Default Values |
|-----------|-------------|----------------|
| **SkillCategory** | Skill type | Technical, Professional, Domain, Leadership, Tools |
| **SkillType** | T-shaped classification | Core, Specialty, Adjacent |
| **AssessmentType** | Assessment type | SelfAssessment, ManagerAssessment, PeerAssessment, Quiz, CodingTest, CaseStudy, RoleBasedTest, SituationalJudgment |
| **AssessmentStatus** | Assessment status | Draft, Pending, InProgress, Completed, Reviewed, Disputed, Resolved |
| **QuestionType** | Question type | MultipleChoice, MultipleAnswer, TrueFalse, ShortAnswer, LongAnswer, CodingChallenge, Scenario, SituationalJudgment, Rating |
| **DifficultyLevel** | Difficulty | Easy, Medium, Hard, Expert |
| **GapPriority** | Gap priority | Low, Medium, High, Critical |
| **LearningResourceType** | Learning resource type | Course, Book, Video, Article, Workshop, Certification, Project, Mentorship, Seminar |
| **LearningPathStatus** | Learning path status | Suggested, Approved, InProgress, Completed, Paused, Cancelled |
| **EmploymentStatus** | Employee status | Active, OnLeave, Resigned, Terminated |
| **UserRole** | System role | Employee, TeamLead, Manager, Admin |
| **SjtEffectiveness** | SJT effectiveness level | MostEffective, Effective, Ineffective, CounterProductive |

### 4.2 SystemEnumValue Entity Structure

```csharp
public class SystemEnumValue
{
    public Guid Id { get; set; }
    public string EnumType { get; set; }      // "SkillCategory", "AssessmentType", etc.
    public int Value { get; set; }            // Numeric value (1, 2, 3...)
    public string Code { get; set; }          // "Technical", "Quiz", etc.
    public string Name { get; set; }          // Display name (English only)
    public string? Description { get; set; }  // Detailed description
    public string? Color { get; set; }        // Color code for UI
    public string? Icon { get; set; }         // Icon name
    public int DisplayOrder { get; set; }     // Display order
    public bool IsActive { get; set; }        // Is currently active
    public bool IsSystem { get; set; }        // System value (cannot delete)
    public string? Metadata { get; set; }     // JSON for additional info
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 4.3 Hard-coded Enums (Unchangeable)

Some enums remain hard-coded due to system logic dependencies:

| Enum | Reason |
|------|--------|
| **ProficiencyLevel** (1-7) | SFIA standard framework, calculation logic depends on numeric values |

---

## 5. QUESTION TYPES

### 5.1 All Supported Question Types (9 types)

| Type | Value | Description | Answer Options | UI Component |
|------|-------|-------------|----------------|--------------|
| **MultipleChoice** | 1 | Single correct answer | Yes (radio) | Radio.Group |
| **MultipleAnswer** | 2 | Multiple correct answers | Yes (checkbox) | Checkbox.Group |
| **TrueFalse** | 3 | True or false | Yes (fixed: True/False) | Radio.Group |
| **ShortAnswer** | 4 | Brief text response | No | Input |
| **LongAnswer** | 5 | Extended text response | No | TextArea |
| **CodingChallenge** | 6 | Code writing/completion | No | TextArea (monospace) |
| **Scenario** | 7 | Scenario-based question | Optional | Radio.Group or TextArea |
| **SituationalJudgment** | 8 | SJT question | Optional | Radio.Group or TextArea |
| **Rating** | 9 | Rating scale (1-5 stars) | No | Rate (5 stars) |

### 5.2 Question Form Behavior

| Type | Options Section | Correct Answer Logic |
|------|-----------------|---------------------|
| MultipleChoice | Show (editable) | Only 1 can be correct |
| MultipleAnswer | Show (editable) | Multiple can be correct |
| TrueFalse | Show (fixed True/False) | Only 1 can be correct |
| ShortAnswer | Hidden | N/A |
| LongAnswer | Hidden | N/A |
| CodingChallenge | Hidden | N/A |
| Scenario | Hidden | N/A |
| SituationalJudgment | Hidden | N/A |
| Rating | Hidden | N/A |

### 5.3 Question Entity Structure

```csharp
public class Question : VersionedEntity
{
    public Guid? SectionId { get; set; }
    public Guid? SkillId { get; set; }           // Optional
    public ProficiencyLevel TargetLevel { get; set; }

    // Content
    public QuestionType Type { get; set; }
    public string Content { get; set; }
    public string? CodeSnippet { get; set; }     // For coding questions
    public string? MediaUrl { get; set; }

    // Scoring
    public int Points { get; set; } = 1;
    public int? TimeLimitSeconds { get; set; }
    public string? GradingRubric { get; set; }   // For manual grading

    // Metadata
    public DifficultyLevel Difficulty { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? AiPromptUsed { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }            // JSON array

    // Navigation
    public ICollection<QuestionOption> Options { get; set; }
}
```

### 5.4 AssessmentResponse Structure

```csharp
public class AssessmentResponse
{
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; }

    // Response content (based on question type)
    public string? TextResponse { get; set; }      // ShortAnswer, LongAnswer, Scenario, SJT
    public string? CodeResponse { get; set; }      // CodingChallenge
    public string? SelectedOptions { get; set; }   // JSON array - MC, MA, TF, Scenario, SJT
    public int? RatingValue { get; set; }          // Rating (1-5)

    // Scoring
    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public int? TimeSpentSeconds { get; set; }
}
```

---

## 6. AI QUESTION GENERATION

### Request Structure
```json
{
  "skillId": "guid (optional)",
  "skillName": "string (optional)",
  "targetLevel": "ProficiencyLevel (optional)",
  "questionCount": 5,
  "questionTypes": [1, 2, 3],
  "assessmentType": "Quiz | CodingTest | CaseStudy | RoleBasedTest | SituationalJudgment",
  "difficulty": "DifficultyLevel (optional)",
  "language": "en",
  "additionalContext": "string (optional)",
  "jobRole": "string (optional)",
  "sectionId": "guid (required for generate-ai endpoint)"
}
```

### Response Structure
```json
{
  "success": true,
  "questions": [
    {
      "content": "Question content",
      "assessmentType": "Quiz",
      "questionType": "MultipleChoice",
      "difficulty": "Medium",
      "targetLevel": "Apply",
      "skillId": "guid",
      "suggestedPoints": 10,
      "suggestedTimeSeconds": 120,
      "tags": ["tag1", "tag2"],
      "explanation": "Answer explanation",

      "options": [
        { "content": "Option A", "isCorrect": true, "explanation": "Because..." }
      ],

      "codeSnippet": "// Template code",
      "expectedOutput": "Expected result",
      "testCases": [
        { "input": "...", "expectedOutput": "...", "isHidden": false }
      ],

      "scenario": "Scenario description...",
      "situation": "Situation description...",
      "responseOptions": [
        { "content": "Option A", "effectiveness": "MostEffective", "explanation": "..." }
      ],

      "expectedAnswer": "Sample answer",
      "gradingRubric": "Grading criteria"
    }
  ],
  "metadata": {
    "model": "claude-3",
    "tokensUsed": 1500,
    "generationTimeMs": 2500,
    "generatedAt": "2026-01-23T10:00:00Z"
  }
}
```

---

## 7. IMPLEMENTATION STATUS

### Completed (Production Ready)

- [x] Skill Taxonomy (Domains, Subcategories, Skills, Levels)
- [x] Test Templates & Sections
- [x] Questions with Full CRUD (Create, Read, Update, Delete)
- [x] All 9 Question Types supported
- [x] AI Question Generation
- [x] Assessment Workflow (Start -> Answer -> Submit -> Result)
- [x] Authentication (Login, Register)
- [x] Dashboard with Skill Matrix
- [x] Dynamic Enum Configuration (Admin)
- [x] Question Form with smart option handling:
  - Auto-generate True/False options
  - Single correct answer for MultipleChoice/TrueFalse
  - Multiple correct answers for MultipleAnswer
  - Auto-cleanup when switching types
- [x] Take Test supports all question types including Rating (5-star)

### Pending Development

- [ ] Job Role Management + UI
- [ ] Role Skill Requirements + UI
- [ ] Team Management + UI
- [ ] Learning Resources + UI
- [ ] Learning Paths with AI recommendations
- [ ] Skill Gap Analysis + Reports
- [ ] Employee Profile page
- [ ] Admin Dashboard
- [ ] Email Notifications
- [ ] Export/Reports (PDF, Excel)

---

## 8. TECHNOLOGY STACK

| Layer | Technology |
|-------|------------|
| Backend | .NET Core 9, Entity Framework Core 9 |
| Database | PostgreSQL |
| Frontend | React 19, TypeScript, Ant Design v6 |
| State Management | TanStack Query v5 (React Query) |
| Form Handling | Ant Design Form with Form.useWatch |
| AI Integration | Mock Service (ready for Claude API) |
| Authentication | Password-based (extensible for JWT/OAuth2) |

---

## 9. KEY COMPONENTS

### 9.1 QuestionFormModal (Frontend)

Location: `web/src/components/QuestionFormModal.tsx`

Features:
- Supports all 9 question types
- Smart option handling based on question type
- Auto-generates True/False options when type = TrueFalse
- Enforces single correct answer for MultipleChoice
- Allows multiple correct answers for MultipleAnswer
- Auto-cleanup options when switching to non-option types
- Form validation with Ant Design Form

### 9.2 TakeTest Page (Frontend)

Location: `web/src/pages/assessments/TakeTest.tsx`

Features:
- Renders appropriate UI for each question type
- Radio.Group for MultipleChoice, TrueFalse
- Checkbox.Group for MultipleAnswer
- Input for ShortAnswer
- TextArea for LongAnswer, Scenario, SJT
- TextArea with monospace font for CodingChallenge
- Rate component (5 stars) for Rating
- Question navigator with answered/unanswered indicators
- Time tracking per question
- Auto-save functionality

### 9.3 TestTemplateDetail (Frontend)

Location: `web/src/pages/tests/TestTemplateDetail.tsx`

Features:
- View template with sections and questions
- Add/Edit/Delete questions via QuestionFormModal
- AI question generation
- Question list with type, difficulty, points display
- Expand to see question options

---

## 10. NEXT STEPS (Priority Order)

1. **HIGH**: Job Role Management
2. **HIGH**: Employee Profile Page
3. **HIGH**: Role Skill Requirements
4. **MEDIUM**: Team Management
5. **MEDIUM**: Learning Resources & Paths
6. **MEDIUM**: Skill Gap Analysis
7. **LOW**: Admin Dashboard
8. **LOW**: Export/Reports (PDF, Excel)
9. **LOW**: Email Notifications

---

## 11. CHANGELOG

### 2026-01-23
- Added Rating question type (value = 9)
- Implemented full Question CRUD (Create, Read, Update, Delete)
- Added smart option handling in QuestionFormModal
- Fixed True/False correct answer selection
- Fixed Multiple Choice single answer enforcement
- Updated TakeTest to support all 9 question types
- Added 5-star rating UI for Rating questions

### 2026-01-22
- Added SituationalJudgment and Scenario question types
- Renamed Essay to LongAnswer
- Made Question.SkillId nullable
- Changed GradingRubric from jsonb to text
