# 📋 SRS - SkillMatrix System

## Tổng Quan Dự Án

**SkillMatrix** là hệ thống quản lý kỹ năng nhân viên, đánh giá năng lực và đề xuất lộ trình học tập. Hệ thống tích hợp framework SFIA 9 và hỗ trợ AI để tự động tạo câu hỏi và phân tích skill gaps.

---

## 1. DOMAIN ENTITIES

### 1.1 Taxonomy (Phân loại Kỹ năng)

| Entity | Mô tả | Status |
|--------|-------|--------|
| **SkillDomain** | Nhóm kỹ năng cấp cao (DEV, STRA, PEOP...) | ✅ Hoàn thành |
| **SkillSubcategory** | Phân nhóm con theo domain | ✅ Hoàn thành |
| **Skill** | Kỹ năng cụ thể với mức độ | ✅ Hoàn thành |
| **SkillLevelDefinition** | Tiêu chí hành vi cho từng level của skill | ✅ Hoàn thành |
| **SkillRelationship** | Quan hệ giữa các skill (prerequisite, related) | ✅ Entity có, UI chưa |
| **ProficiencyLevelDefinition** | Định nghĩa mức độ SFIA 9 (1-7) | ✅ Hoàn thành |

### 1.2 Organization (Tổ chức)

| Entity | Mô tả | Status |
|--------|-------|--------|
| **Team** | Phòng ban/nhóm (hỗ trợ hierarchy) | ✅ Entity có, API chưa đầy đủ |
| **JobRole** | Vị trí công việc (BE, FE, QA, BA, PM...) | ✅ Entity có, API chưa |
| **RoleSkillRequirement** | Yêu cầu skill cho từng role | ✅ Entity có, API chưa |
| **Employee** | Nhân viên với profile đầy đủ | ✅ Hoàn thành |
| **EmployeeSkill** | Skill hiện tại của nhân viên | ✅ Hoàn thành |
| **EmployeeSkillHistory** | Lịch sử thay đổi skill level | ✅ Hoàn thành |

### 1.3 Project (Dự án)

| Entity | Mô tả | Status |
|--------|-------|--------|
| **Project** | Dự án với skill requirements | 🔲 Entity có, API chưa |
| **ProjectSkillRequirement** | Skills cần cho dự án | 🔲 Entity có, API chưa |
| **ProjectAssignment** | Phân công nhân viên vào dự án | 🔲 Entity có, API chưa |

### 1.4 Assessment (Đánh giá)

| Entity | Mô tả | Status |
|--------|-------|--------|
| **TestTemplate** | Template bài test có thể tái sử dụng | ✅ Hoàn thành |
| **TestSection** | Sections trong test | ✅ Hoàn thành |
| **Question** | Câu hỏi với nhiều loại | ✅ Hoàn thành |
| **QuestionOption** | Options cho trắc nghiệm | ✅ Hoàn thành |
| **Assessment** | Phiên đánh giá | ✅ Hoàn thành |
| **AssessmentSkillResult** | Kết quả theo skill | ✅ Hoàn thành |
| **AssessmentResponse** | Câu trả lời của nhân viên | ✅ Hoàn thành |

### 1.5 Learning (Học tập)

| Entity | Mô tả | Status |
|--------|-------|--------|
| **LearningResource** | Tài liệu học (Course, Book, Cert...) | 🔲 Entity có, API chưa |
| **LearningResourceSkill** | Skills mà resource phát triển | 🔲 Entity có, API chưa |
| **EmployeeLearningPath** | Lộ trình học cá nhân (AI-generated) | 🔲 Entity có, API chưa |
| **LearningPathItem** | Items trong lộ trình | 🔲 Entity có, API chưa |
| **SkillGap** | Khoảng cách skill cần phát triển | 🔲 Entity có, API chưa |
| **TeamSkillGap** | Skill gaps cấp team | 🔲 Entity có, API chưa |

### 1.6 Configuration (Cấu hình - Dynamic Enums)

| Entity | Mô tả | Status |
|--------|-------|--------|
| **SystemEnumValue** | Giá trị enum có thể cấu hình từ Admin | ✅ Hoàn thành |

---

## 2. API ENDPOINTS

### 2.1 Taxonomy Management

```
✅ GET/POST/PUT/DELETE /api/skills                    - CRUD Skills
✅ GET/POST/PUT/DELETE /api/skilldomains              - CRUD Domains
✅ GET/POST/PUT/DELETE /api/skillsubcategories        - CRUD Subcategories
✅ GET/POST/PUT/DELETE /api/leveldefinitions          - CRUD Level Definitions
✅ POST /api/leveldefinitions/seed                    - Seed SFIA 9 defaults
✅ GET /api/enums/*                                   - Get all enums
```

### 2.2 Assessment & Testing

```
✅ GET/POST/PUT/DELETE /api/testtemplates             - CRUD Templates
✅ POST/PUT/DELETE /api/testtemplates/sections        - Manage sections
✅ GET/POST/PUT/DELETE /api/questions                 - CRUD Questions
✅ POST /api/questions/bulk                           - Bulk create
✅ POST /api/questions/generate-ai                    - AI generation
✅ GET /api/assessments/available/{employeeId}        - Available tests
✅ POST /api/assessments/start                        - Start assessment
✅ GET /api/assessments/{id}/continue                 - Continue test
✅ POST /api/assessments/answer                       - Submit answer
✅ POST /api/assessments/{id}/submit                  - Complete test
✅ GET /api/assessments/{id}/result                   - Get result
```

### 2.3 Authentication

```
✅ POST /api/auth/login                               - Login
✅ POST /api/auth/register                            - Register
✅ GET /api/auth/me/{userId}                          - Get current user
✅ POST /api/auth/change-password/{userId}            - Change password
✅ GET /api/auth/users                                - List users (admin)
✅ POST /api/auth/seed                                - Seed default users
```

### 2.4 Dashboard

```
✅ GET /api/dashboard/overview                        - Statistics
✅ GET /api/dashboard/employees/skills                - All employees skills
✅ GET /api/dashboard/employees/{id}/skills           - Single employee
✅ GET /api/dashboard/skill-matrix                    - Team skill matrix
```

### 2.5 AI Services

```
✅ POST /api/ai/generate-questions                    - Generate questions
✅ POST /api/ai/grade-answer                          - Grade answer
✅ POST /api/ai/analyze-skill-gaps                    - Analyze gaps
```

### 2.6 Configuration (Admin) - Hoàn thành

```
✅ GET /api/systemenums/types                         - Get all enum types
✅ GET /api/systemenums/values/{enumType}             - Get values for enum type
✅ GET /api/systemenums/dropdown/{enumType}           - Get dropdown values
✅ GET /api/systemenums/{id}                          - Get single value
✅ POST /api/systemenums                              - Create enum value
✅ PUT /api/systemenums/{id}                          - Update enum value
✅ DELETE /api/systemenums/{id}                       - Delete enum value
✅ PATCH /api/systemenums/{id}/toggle-active          - Toggle active
✅ POST /api/systemenums/reorder                      - Reorder values
✅ POST /api/systemenums/seed                         - Seed default values
```

### 2.7 Chưa có API (Cần phát triển)

```
🔲 /api/jobroles                                      - Job Role management
🔲 /api/roleskillrequirements                         - Role skill requirements
🔲 /api/teams                                         - Team management
🔲 /api/projects                                      - Project management
🔲 /api/learningresources                             - Learning resources
🔲 /api/learningpaths                                 - Learning paths
🔲 /api/skillgaps                                     - Skill gap analysis
```

---

## 3. FRONTEND PAGES

### 3.1 Đã hoàn thành

| Page | Đường dẫn | Mô tả |
|------|-----------|-------|
| Login | `/login` | Đăng nhập |
| Dashboard | `/dashboard` | Tổng quan, skill matrix |
| Available Tests | `/tests` | Danh sách bài test |
| Take Test | `/tests/:id/take` | Làm bài test |
| Test Result | `/tests/:id/result` | Kết quả test |
| Skill Domains | `/taxonomy/domains` | Quản lý domains |
| Subcategories | `/taxonomy/subcategories` | Quản lý subcategories |
| Skills | `/taxonomy/skills` | Quản lý skills |
| Level Definitions | `/taxonomy/levels` | Quản lý mức độ |
| Test Templates | `/templates` | Quản lý templates |
| Template Detail | `/templates/:id` | Chi tiết template |

### 3.2 Cần phát triển

| Page | Mô tả | Priority |
|------|-------|----------|
| ~~**System Enums**~~ | ~~Quản lý dynamic enums~~ | ✅ Hoàn thành (`/admin/enums`) |
| Job Roles | Quản lý vị trí công việc | HIGH |
| Role Requirements | Yêu cầu skill cho role | HIGH |
| Team Management | Quản lý team/phòng ban | MEDIUM |
| Learning Resources | Quản lý tài liệu học | MEDIUM |
| Learning Paths | Lộ trình học cá nhân | MEDIUM |
| Employee Profile | Profile chi tiết nhân viên | HIGH |
| Skill Gap Report | Báo cáo skill gaps | HIGH |
| Admin Dashboard | Quản trị hệ thống | MEDIUM |

---

## 4. DYNAMIC ENUMERATIONS (Configurable)

### 4.1 Enum Types (Cấu hình từ Admin)

Các enum sau sẽ được lưu trong database và có thể cấu hình từ Admin:

| Enum Type | Mô tả | Default Values |
|-----------|-------|----------------|
| **SkillCategory** | Loại kỹ năng | Technical, Professional, Domain, Leadership, Tools |
| **SkillType** | Phân loại T-shaped | Core, Specialty, Adjacent |
| **AssessmentType** | Loại đánh giá | SelfAssessment, ManagerAssessment, PeerAssessment, Quiz, CodingTest, CaseStudy, RoleBasedTest, SituationalJudgment |
| **AssessmentStatus** | Trạng thái đánh giá | Draft, Pending, InProgress, Completed, Reviewed, Disputed, Resolved |
| **QuestionType** | Loại câu hỏi | MultipleChoice, MultipleAnswer, TrueFalse, ShortAnswer, LongAnswer, CodingChallenge, Scenario, SituationalJudgment |
| **DifficultyLevel** | Độ khó | Easy, Medium, Hard, Expert |
| **GapPriority** | Mức ưu tiên gap | Low, Medium, High, Critical |
| **LearningResourceType** | Loại tài liệu học | Course, Book, Video, Article, Workshop, Certification, Project, Mentorship, Seminar |
| **LearningPathStatus** | Trạng thái lộ trình | Suggested, Approved, InProgress, Completed, Paused, Cancelled |
| **EmploymentStatus** | Trạng thái nhân viên | Active, OnLeave, Resigned, Terminated |
| **UserRole** | Vai trò hệ thống | Employee, TeamLead, Manager, Admin |
| **SjtEffectiveness** | Mức hiệu quả SJT | MostEffective, Effective, Ineffective, CounterProductive |

### 4.2 SystemEnumValue Entity Structure

```csharp
public class SystemEnumValue
{
    public Guid Id { get; set; }
    public string EnumType { get; set; }      // "SkillCategory", "AssessmentType", etc.
    public int Value { get; set; }            // Numeric value (1, 2, 3...)
    public string Code { get; set; }          // "Technical", "Quiz", etc.
    public string Name { get; set; }          // Display name (có thể đa ngôn ngữ)
    public string? NameVi { get; set; }       // Tên tiếng Việt
    public string? Description { get; set; }  // Mô tả chi tiết
    public string? Color { get; set; }        // Color code for UI
    public string? Icon { get; set; }         // Icon name
    public int DisplayOrder { get; set; }     // Thứ tự hiển thị
    public bool IsActive { get; set; }        // Có đang active không
    public bool IsSystem { get; set; }        // System value (không thể xóa)
    public string? Metadata { get; set; }     // JSON cho thông tin bổ sung
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 4.3 Hard-coded Enums (Không thay đổi)

Một số enum vẫn giữ hard-code vì ảnh hưởng đến logic hệ thống:

| Enum | Lý do |
|------|-------|
| **ProficiencyLevel** (1-7) | SFIA framework chuẩn, logic tính toán dựa vào giá trị số |

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
  "language": "vi | en",
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
      "content": "Nội dung câu hỏi",
      "assessmentType": "Quiz",
      "questionType": "MultipleChoice",
      "difficulty": "Medium (optional)",
      "targetLevel": "Apply (optional)",
      "skillId": "guid (optional)",
      "skillName": "string (optional)",
      "suggestedPoints": 10,
      "suggestedTimeSeconds": 120,
      "tags": ["tag1", "tag2"],
      "explanation": "Giải thích đáp án",

      // Cho Quiz
      "options": [
        { "content": "Option A", "isCorrect": true, "explanation": "Vì..." }
      ],

      // Cho Coding Test
      "codeSnippet": "// Template code",
      "expectedOutput": "Expected result",
      "testCases": [
        { "input": "...", "expectedOutput": "...", "isHidden": false }
      ],

      // Cho Case Study
      "scenario": "Mô tả tình huống...",
      "documents": ["doc1.pdf"],

      // Cho Role-based Test
      "roleContext": "Context về vai trò...",
      "responsibilities": ["Trách nhiệm 1"],

      // Cho SJT
      "situation": "Mô tả tình huống...",
      "responseOptions": [
        { "content": "Phương án A", "effectiveness": "MostEffective", "explanation": "..." }
      ],

      // Cho tự luận
      "expectedAnswer": "Câu trả lời mẫu",
      "gradingRubric": "Tiêu chí chấm điểm"
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

### ✅ Hoàn thành (Production Ready)

- [x] Skill Taxonomy (Domains, Subcategories, Skills, Levels)
- [x] Test Templates & Sections
- [x] Questions với AI Generation
- [x] Assessment Workflow (Start → Answer → Submit → Result)
- [x] Authentication (Login, Register)
- [x] Dashboard với Skill Matrix
- [x] **Dynamic Enum Configuration (Admin)** - NEW!
- [x] API cho tất cả features trên
- [x] Frontend pages cho features trên

### 🔲 Cần phát triển

- [x] ~~Dynamic Enum Configuration (Admin)~~ - DONE!
- [ ] Job Role Management + UI
- [ ] Role Skill Requirements + UI
- [ ] Team Management + UI
- [ ] Learning Resources + UI
- [ ] Learning Paths với AI recommendations
- [ ] Skill Gap Analysis + Reports
- [ ] Employee Profile page chi tiết
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
| AI Integration | Mock Service (sẵn sàng cho Claude API) |
| Authentication | Password-based (extensible cho JWT/OAuth2) |

---

## 8. NEXT STEPS (Priority Order)

1. **HIGH**: Implement Dynamic Enum Configuration
2. **HIGH**: Job Role Management
3. **HIGH**: Employee Profile Page
4. **MEDIUM**: Team Management
5. **MEDIUM**: Learning Resources & Paths
6. **LOW**: Admin Dashboard
7. **LOW**: Export/Reports
