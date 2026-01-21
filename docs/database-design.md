# Skill Matrix Database Design

## Overview

Thiết kế database dựa trên **SFIA 9 (Skills Framework for the Information Age)** với khả năng mở rộng cao cho các nhu cầu đặc thù của công ty.

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            SKILL TAXONOMY (SFIA-based)                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐    ┌──────────────────┐    ┌─────────────┐                │
│  │ SkillDomain  │───<│ SkillSubcategory │───<│   Skill     │                │
│  │              │    │                  │    │             │                │
│  │ - DEV        │    │ - PROG           │    │ - C#        │                │
│  │ - ARCH       │    │ - FRMW           │    │ - React     │                │
│  │ - QUAL       │    │ - TEST           │    │ - SQL       │                │
│  │ - DATA       │    │ - MLAI           │    │ - ...       │                │
│  │ - PEOP       │    │ - SOFT           │    │             │                │
│  │ - TOOL       │    │ - ...            │    └──────┬──────┘                │
│  └──────────────┘    └──────────────────┘           │                       │
│                                                      │                       │
│                           ┌──────────────────────────┴──────────┐            │
│                           │                                     │            │
│                    ┌──────▼──────────────┐    ┌─────────────────▼─────┐     │
│                    │ SkillLevelDefinition│    │  SkillRelationship    │     │
│                    │ (SFIA Levels 1-7)   │    │ (Prerequisites, etc.) │     │
│                    └─────────────────────┘    └───────────────────────┘     │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                              ORGANIZATION                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌────────────────────────┐         │
│  │    Team      │───<│   Employee   │───<│    EmployeeSkill       │         │
│  │              │    │              │    │  (Current skill state) │         │
│  │ - SoEzy      │    │ - FullName   │    │                        │         │
│  │ - Mezy       │    │ - JobRole    │    │ - CurrentLevel         │         │
│  └──────────────┘    │ - Manager    │    │ - SelfAssessed         │         │
│         │            │ - Status     │    │ - ManagerAssessed      │         │
│         │            └──────┬───────┘    │ - TestValidated        │         │
│         │                   │            └────────────────────────┘         │
│         │                   │                                               │
│  ┌──────▼──────┐    ┌──────▼────────────┐                                   │
│  │   Project   │    │  JobRole          │                                   │
│  │             │    │  (Career Ladder)  │                                   │
│  │             │    │                   │                                   │
│  └──────┬──────┘    │ - Junior BE       │                                   │
│         │           │ - Mid BE          │                                   │
│  ┌──────▼───────┐   │ - Senior BE       │                                   │
│  │ProjectSkill  │   │ - Lead BE         │                                   │
│  │Requirements  │   └────────┬──────────┘                                   │
│  └──────────────┘            │                                              │
│                       ┌──────▼──────────────┐                               │
│                       │ RoleSkillRequirement│                               │
│                       │ (Skill expectations │                               │
│                       │  for each level)    │                               │
│                       └─────────────────────┘                               │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                              ASSESSMENT                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐    ┌──────────────────┐    ┌───────────────────┐      │
│  │   TestTemplate   │───<│   TestSection    │───<│    Question       │      │
│  │                  │    │                  │    │                   │      │
│  │ - Quiz           │    │ - Programming    │    │ - MultipleChoice  │      │
│  │ - CodingTest     │    │ - Case Study     │    │ - CodingChallenge │      │
│  │ - RoleBasedTest  │    │ - Situational    │    │ - CaseStudy       │      │
│  │ - SJT            │    │                  │    │ - BehavioralAnch. │      │
│  └────────┬─────────┘    └──────────────────┘    └─────────┬─────────┘      │
│           │                                                 │                │
│           │                                          ┌──────▼──────────┐    │
│           │                                          │  QuestionOption │    │
│           │                                          └─────────────────┘    │
│           │                                                                  │
│  ┌────────▼─────────┐    ┌───────────────────────┐                          │
│  │   Assessment     │───<│ AssessmentSkillResult │                          │
│  │                  │    │                       │                          │
│  │ - SelfAssessment │    │ - AssessedLevel       │                          │
│  │ - ManagerAssess. │    │ - AiExplanation       │                          │
│  │ - TestBased      │    │ - Evidence            │                          │
│  │                  │    └───────────────────────┘                          │
│  │ - AiAnalysis     │                                                       │
│  │ - AiRecommend.   │    ┌───────────────────────┐                          │
│  │ - EmployeeFdbk   │───<│  AssessmentResponse   │                          │
│  └──────────────────┘    │  (Answers to quest.)  │                          │
│                          └───────────────────────┘                          │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                           LEARNING & GAP ANALYSIS                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐                     ┌─────────────────────────┐       │
│  │    SkillGap      │                     │   LearningResource      │       │
│  │                  │                     │                         │       │
│  │ - CurrentLevel   │                     │ - Course, Book, Video   │       │
│  │ - RequiredLevel  │                     │ - Workshop, Seminar     │       │
│  │ - GapSize        │                     │ - Certification         │       │
│  │ - Priority       │                     │ - Project (hands-on)    │       │
│  │ - AiAnalysis     │                     │                         │       │
│  │ - AiRecommend.   │                     │ - AiSuggested           │       │
│  └────────┬─────────┘                     └───────────┬─────────────┘       │
│           │                                           │                      │
│           │         ┌─────────────────────────┐       │                      │
│           └────────>│  EmployeeLearningPath   │<──────┘                      │
│                     │                         │                              │
│                     │ - AI Generated          │                              │
│                     │ - Manager Approved      │                              │
│                     │ - Progress Tracking     │                              │
│                     │                         │                              │
│                     └───────────┬─────────────┘                              │
│                                 │                                            │
│                     ┌───────────▼─────────────┐                              │
│                     │   LearningPathItem      │                              │
│                     │                         │                              │
│                     │ - DisplayOrder          │                              │
│                     │ - Status                │                              │
│                     │ - CompletedAt           │                              │
│                     └─────────────────────────┘                              │
│                                                                              │
│  ┌──────────────────┐                                                        │
│  │  TeamSkillGap    │  (Aggregate team-level gap analysis)                  │
│  └──────────────────┘                                                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## SFIA Proficiency Levels

| Level | Name | Description (Vietnamese) |
|-------|------|--------------------------|
| 0 | None | Không có kiến thức/kinh nghiệm |
| 1 | Follow | Làm theo hướng dẫn, đang học |
| 2 | Assist | Hỗ trợ người khác, phát triển kỹ năng |
| 3 | Apply | Áp dụng độc lập, hiểu best practices |
| 4 | Enable | Hỗ trợ người khác, đảm bảo chất lượng |
| 5 | Ensure/Advise | Đảm bảo/Tư vấn ở cấp tổ chức |
| 6 | Initiate | Khởi xướng, ảnh hưởng chiến lược |
| 7 | Set Strategy | Định hướng chiến lược, dẫn đầu ngành |

## Key Design Decisions

### 1. Versioned Entities
- `SkillDomain`, `SkillSubcategory`, `Skill`, `JobRole` đều kế thừa `VersionedEntity`
- Cho phép track thay đổi theo thời gian
- `IsCurrent` flag để lấy version hiện tại
- `EffectiveFrom`, `EffectiveTo` cho version control

### 2. Skill Level Definitions
- Mỗi skill có định nghĩa chi tiết cho từng level (SFIA style)
- `BehavioralIndicators`: Hành vi quan sát được
- `EvidenceExamples`: Ví dụ bằng chứng
- Giúp assessment khách quan hơn

### 3. Multiple Assessment Sources
- `EmployeeSkill` lưu trữ:
  - `CurrentLevel`: Level hiện tại (tổng hợp)
  - `SelfAssessedLevel`: Tự đánh giá
  - `ManagerAssessedLevel`: Manager đánh giá
  - `TestValidatedLevel`: Qua bài test
- Hybrid approach cho accuracy cao hơn

### 4. AI Integration Points
- `Assessment.AiAnalysis`: Phân tích của AI
- `Assessment.AiRecommendations`: Đề xuất của AI
- `AssessmentSkillResult.AiExplanation`: Giải thích tại sao AI đánh giá mức này
- `SkillGap.AiRecommendation`: AI đề xuất cách giải quyết gap
- `EmployeeLearningPath.AiRationale`: Lý do AI suggest learning path
- `Question.IsAiGenerated`: Track câu hỏi do AI tạo

### 5. Soft Delete & Audit
- Tất cả entities đều có soft delete (`IsDeleted`)
- Audit fields: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- Global query filters tự động exclude deleted records

### 6. Career Ladder Integration
- `JobRole` có hierarchy (Junior -> Mid -> Senior -> Lead)
- `RoleSkillRequirement` định nghĩa skill cần thiết cho mỗi role
- `MinimumLevel` vs `ExpectedLevel` cho flexibility

### 7. T-Shaped Model Support
- `SkillType` enum: Core, Specialty, Adjacent
- Core: Everyone needs (Git, Communication)
- Specialty: Role-specific deep skills
- Adjacent: Nice to have, cross-functional

## Tables Summary

| Module | Table | Purpose |
|--------|-------|---------|
| Taxonomy | SkillDomains | Top-level skill categories |
| Taxonomy | SkillSubcategories | Second-level grouping |
| Taxonomy | Skills | Individual skills |
| Taxonomy | SkillLevelDefinitions | SFIA level descriptions |
| Taxonomy | SkillRelationships | Prerequisites, related skills |
| Organization | Teams | Team structure |
| Organization | JobRoles | Career ladder |
| Organization | RoleSkillRequirements | Skill expectations per role |
| Organization | Employees | User profiles |
| Organization | EmployeeSkills | Current skill state |
| Organization | EmployeeSkillHistories | Skill change history |
| Organization | Projects | Project tracking |
| Organization | ProjectSkillRequirements | Skills needed for projects |
| Organization | ProjectAssignments | Employee-project mapping |
| Assessment | TestTemplates | Reusable test definitions |
| Assessment | TestSections | Test sections |
| Assessment | Questions | Test questions |
| Assessment | QuestionOptions | Multiple choice options |
| Assessment | Assessments | Assessment sessions |
| Assessment | AssessmentSkillResults | Skill results from assessment |
| Assessment | AssessmentResponses | Employee answers |
| Learning | SkillGaps | Individual skill gaps |
| Learning | TeamSkillGaps | Team-level gap analysis |
| Learning | LearningResources | Learning materials |
| Learning | LearningResourceSkills | Resource-skill mapping |
| Learning | EmployeeLearningPaths | Personalized learning paths |
| Learning | LearningPathItems | Learning path steps |

## Tech Stack

- **Backend**: .NET Core
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **JSON columns**: PostgreSQL `jsonb` type for flexible data (Tags, BehavioralIndicators, etc.)
