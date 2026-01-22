# Skill Matrix Database Design

## Overview

Database design based on **SFIA 9 (Skills Framework for the Information Age)** with high extensibility for company-specific needs.

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

| Level | Name | Description |
|-------|------|-------------|
| 0 | None | No knowledge or experience |
| 1 | Follow | Following instructions, learning |
| 2 | Assist | Assisting others, developing skills |
| 3 | Apply | Applying independently, understanding best practices |
| 4 | Enable | Enabling others, ensuring quality |
| 5 | Ensure/Advise | Ensuring/Advising at organizational level |
| 6 | Initiate | Initiating, influencing strategy |
| 7 | Set Strategy | Setting strategy, leading the industry |

## Key Design Decisions

### 1. Versioned Entities
- `SkillDomain`, `SkillSubcategory`, `Skill`, `JobRole` all inherit from `VersionedEntity`
- Allows tracking changes over time
- `IsCurrent` flag to get current version
- `EffectiveFrom`, `EffectiveTo` for version control

### 2. Skill Level Definitions
- Each skill has detailed definitions for each level (SFIA style)
- `BehavioralIndicators`: Observable behaviors
- `EvidenceExamples`: Evidence examples
- Helps make assessment more objective

### 3. Multiple Assessment Sources
- `EmployeeSkill` stores:
  - `CurrentLevel`: Current level (aggregated)
  - `SelfAssessedLevel`: Self-assessment
  - `ManagerAssessedLevel`: Manager assessment
  - `TestValidatedLevel`: Test-based validation
- Hybrid approach for higher accuracy

### 4. AI Integration Points
- `Assessment.AiAnalysis`: AI analysis
- `Assessment.AiRecommendations`: AI recommendations
- `AssessmentSkillResult.AiExplanation`: Explanation of why AI assessed this level
- `SkillGap.AiRecommendation`: AI recommendation for addressing the gap
- `EmployeeLearningPath.AiRationale`: Reason AI suggested this learning path
- `Question.IsAiGenerated`: Track AI-generated questions

### 5. Soft Delete & Audit
- All entities have soft delete (`IsDeleted`)
- Audit fields: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- Global query filters automatically exclude deleted records

### 6. Career Ladder Integration
- `JobRole` has hierarchy (Junior -> Mid -> Senior -> Lead)
- `RoleSkillRequirement` defines required skills for each role
- `MinimumLevel` vs `ExpectedLevel` for flexibility

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

- **Backend**: .NET Core 9
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 9
- **JSON columns**: PostgreSQL `jsonb` type for flexible data (Tags, BehavioralIndicators, etc.)
