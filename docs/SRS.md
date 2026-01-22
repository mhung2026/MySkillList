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
| **SkillDomain** | High-level skill groups (DEV, STRA, PEOP...) | Done |
| **SkillSubcategory** | Sub-groups within domain | Done |
| **Skill** | Specific skill with levels | Done |
| **SkillLevelDefinition** | Behavioral criteria for each skill level | Done |
| **SkillRelationship** | Relationships between skills (prerequisite, related) | Entity exists, UI pending |
| **ProficiencyLevelDefinition** | SFIA 9 level definitions (1-7) | Done |

### 1.2 Organization

| Entity | Description | Status |
|--------|-------------|--------|
| **Team** | Department/team (supports hierarchy) | Entity exists, API incomplete |
| **JobRole** | Job positions (BE, FE, QA, BA, PM...) | Entity exists, API pending |
| **RoleSkillRequirement** | Skill requirements per role | Entity exists, API pending |
| **Employee** | Employee with full profile | Done |
| **EmployeeSkill** | Employee's current skills | Done |
| **EmployeeSkillHistory** | Skill level change history | Done |

### 1.3 Project

| Entity | Description | Status |
|--------|-------------|--------|
| **Project** | Project with skill requirements | Entity exists, API pending |
| **ProjectSkillRequirement** | Skills needed for project | Entity exists, API pending |
| **ProjectAssignment** | Employee project assignments | Entity exists, API pending |

### 1.4 Assessment

| Entity | Description | Status |
|--------|-------------|--------|
| **TestTemplate** | Reusable test templates | Done |
| **TestSection** | Sections within test | Done |
| **Question** | Questions with multiple types | Done |
| **QuestionOption** | Options for multiple choice | Done |
| **Assessment** | Assessment session | Done |
| **AssessmentSkillResult** | Results per skill | Done |
| **AssessmentResponse** | Employee responses | Done |

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
| **SystemEnumValue** | Configurable enum values from Admin | Done |

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
GET/POST/PUT/DELETE /api/testtemplates             - CRUD Templates
POST/PUT/DELETE /api/testtemplates/sections        - Manage sections
GET/POST/PUT/DELETE /api/questions                 - CRUD Questions
POST /api/questions/bulk                           - Bulk create
POST /api/questions/generate-ai                    - AI generation
GET /api/assessments/available/{employeeId}        - Available tests
POST /api/assessments/start                        - Start assessment
GET /api/assessments/{id}/continue                 - Continue test
POST /api/assessments/answer                       - Submit answer
POST /api/assessments/{id}/submit                  - Complete test
GET /api/assessments/{id}/result                   - Get result
```

### 2.3 Authentication

```
POST /api/auth/login                               - Login
POST /api/auth/register                            - Register
GET /api/auth/me/{userId}                          - Get current user
POST /api/auth/change-password/{userId}            - Change password
GET /api/auth/users                                - List users (admin)
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
GET /api/systemenums/types                         - Get all enum types
GET /api/systemenums/values/{enumType}             - Get values for enum type
GET /api/systemenums/dropdown/{enumType}           - Get dropdown values
GET /api/systemenums/{id}                          - Get single value
POST /api/systemenums                              - Create enum value
PUT /api/systemenums/{id}                          - Update enum value
DELETE /api/systemenums/{id}                       - Delete enum value
PATCH /api/systemenums/{id}/toggle-active          - Toggle active
POST /api/systemenums/reorder                      - Reorder values
POST /api/systemenums/seed                         - Seed default values
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
| Available Tests | `/tests` | Test list |
| Take Test | `/tests/:id/take` | Take a test |
| Test Result | `/tests/:id/result` | Test results |
| Skill Domains | `/taxonomy/domains` | Manage domains |
| Subcategories | `/taxonomy/subcategories` | Manage subcategories |
| Skills | `/taxonomy/skills` | Manage skills |
| Level Definitions | `/taxonomy/levels` | Manage levels |
| Test Templates | `/templates` | Manage templates |
| Template Detail | `/templates/:id` | Template details |
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
| **QuestionType** | Question type | MultipleChoice, MultipleAnswer, TrueFalse, ShortAnswer, LongAnswer, CodingChallenge, Scenario, SituationalJudgment |
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

## 5. AI QUESTION GENERATION

### Request Structure
```json
{
  "skillId": "guid (optional)",
  "skillName": "string (optional)",
  "targetLevel": "ProficiencyLevel (optional)",
  "questionCount": 5,
  "assessmentType": "Quiz | CodingTest | CaseStudy | RoleBasedTest | SituationalJudgment",
  "difficulty": "DifficultyLevel (optional)",
  "language": "en",
  "additionalContext": "string (optional)",
  "jobRole": "string (optional)",
  "sectionId": "guid (optional)"
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
      "difficulty": "Medium (optional)",
      "targetLevel": "Apply (optional)",
      "skillId": "guid (optional)",
      "skillName": "string (optional)",
      "suggestedPoints": 10,
      "suggestedTimeSeconds": 120,
      "tags": ["tag1", "tag2"],
      "explanation": "Answer explanation",

      // For Quiz
      "options": [
        { "content": "Option A", "isCorrect": true, "explanation": "Because..." }
      ],

      // For Coding Test
      "codeSnippet": "// Template code",
      "expectedOutput": "Expected result",
      "testCases": [
        { "input": "...", "expectedOutput": "...", "isHidden": false }
      ],

      // For Case Study
      "scenario": "Scenario description...",
      "documents": ["doc1.pdf"],

      // For Role-based Test
      "roleContext": "Role context...",
      "responsibilities": ["Responsibility 1"],

      // For SJT
      "situation": "Situation description...",
      "responseOptions": [
        { "content": "Option A", "effectiveness": "MostEffective", "explanation": "..." }
      ],

      // For essay questions
      "expectedAnswer": "Sample answer",
      "gradingRubric": "Grading criteria"
    }
  ],
  "metadata": {
    "model": "claude-3",
    "tokensUsed": 1500,
    "generationTimeMs": 2500,
    "generatedAt": "2026-01-21T10:00:00Z"
  }
}
```

---

## 6. IMPLEMENTATION STATUS

### Completed (Production Ready)

- [x] Skill Taxonomy (Domains, Subcategories, Skills, Levels)
- [x] Test Templates & Sections
- [x] Questions with AI Generation
- [x] Assessment Workflow (Start -> Answer -> Submit -> Result)
- [x] Authentication (Login, Register)
- [x] Dashboard with Skill Matrix
- [x] Dynamic Enum Configuration (Admin)
- [x] API for all features above
- [x] Frontend pages for features above

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

## 7. TECHNOLOGY STACK

| Layer | Technology |
|-------|------------|
| Backend | .NET Core 9, Entity Framework Core 9 |
| Database | PostgreSQL |
| Frontend | React 19, TypeScript, Ant Design v6 |
| State Management | TanStack Query (React Query) |
| AI Integration | Mock Service (ready for Claude API) |
| Authentication | Password-based (extensible for JWT/OAuth2) |

---

## 8. NEXT STEPS (Priority Order)

1. **HIGH**: Job Role Management
2. **HIGH**: Employee Profile Page
3. **MEDIUM**: Team Management
4. **MEDIUM**: Learning Resources & Paths
5. **LOW**: Admin Dashboard
6. **LOW**: Export/Reports
