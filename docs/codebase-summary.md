# SkillMatrix Codebase Summary

## Directory Structure

```
SkillMatrix/
├── src/                                    # Backend (.NET 9.0)
│   ├── SkillMatrix.Api/                   # ASP.NET Core API Layer
│   │   ├── Controllers/                   # 13 API controllers
│   │   ├── Program.cs                     # Dependency injection, middleware
│   │   ├── appsettings.json               # Configuration
│   │   └── appsettings.Production.json    # Production settings
│   │
│   ├── SkillMatrix.Application/           # Application Layer (Business Logic)
│   │   ├── Services/                      # 9+ domain services
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   ├── Mappings/                      # AutoMapper configurations
│   │   └── Validators/                    # FluentValidation rules
│   │
│   ├── SkillMatrix.Domain/                # Domain Layer (Core Logic)
│   │   ├── Entities/                      # 50+ domain entities
│   │   ├── Enums/                         # System enumerations
│   │   ├── Interfaces/                    # Contracts
│   │   └── ValueObjects/                  # Domain value objects
│   │
│   └── SkillMatrix.Infrastructure/        # Infrastructure Layer
│       ├── Data/                          # DbContext, migrations
│       ├── Repositories/                  # Data access patterns
│       ├── Services/                      # External integrations
│       └── Seeders/                       # Database seeders
│
├── web/                                   # Frontend (React 19 + TypeScript)
│   ├── src/
│   │   ├── pages/                         # Page components (15+ pages)
│   │   │   ├── auth/
│   │   │   ├── dashboard/
│   │   │   ├── taxonomy/
│   │   │   ├── assessment/
│   │   │   └── admin/
│   │   │
│   │   ├── components/                    # Reusable UI components
│   │   ├── api/                           # API client layer
│   │   ├── hooks/                         # Custom React hooks
│   │   ├── types/                         # TypeScript interfaces
│   │   ├── styles/                        # Global styles (Tailwind/Ant Design)
│   │   ├── utils/                         # Utility functions
│   │   └── App.tsx                        # Main app component
│   │
│   ├── dist/                              # Production build output
│   ├── vite.config.ts                     # Vite build configuration
│   ├── tsconfig.json                      # TypeScript configuration
│   └── package.json                       # Dependencies
│
├── tools/                                 # Utility scripts
│   └── SeedDatabase/                      # Database initialization
│
├── docs/                                  # Documentation
│   ├── README.md                          # Project overview
│   ├── project-overview-pdr.md            # PDR & vision
│   ├── codebase-summary.md                # This file
│   ├── code-standards.md                  # Conventions
│   ├── system-architecture.md             # Architecture
│   ├── database-design.md                 # Database schema
│   ├── project-roadmap.md                 # Roadmap
│   └── SRS.md                             # Requirements
│
├── DEPLOY_GUIDE.md                        # IIS deployment
├── publish.bat                            # Build automation
└── SkillMatrix.sln                        # Visual Studio solution
```

## Backend Architecture

### Layer Breakdown

#### 1. API Layer (SkillMatrix.Api)
**Responsibility:** HTTP request handling, routing, middleware orchestration

**Key Components:**
- **Controllers (13):** Handle HTTP requests/responses
  - `AuthController` - Authentication endpoints
  - `SkillsController` - Skill management
  - `TestTemplatesController` - Test template CRUD
  - `QuestionsController` - Question management
  - `AssessmentsController` - Assessment workflow
  - `DashboardController` - Analytics endpoints
  - `SystemEnumsController` - Configuration management
  - And others for employees, domains, subcategories, levels, etc.

- **Middleware:**
  - Authentication middleware
  - CORS configuration
  - Error handling middleware
  - Logging middleware

- **Configuration:**
  - Dependency injection in Program.cs
  - Database connection setup
  - Service registration

**Technologies:** ASP.NET Core 9, Swagger/OpenAPI

#### 2. Application Layer (SkillMatrix.Application)
**Responsibility:** Business logic, service orchestration, data transformation

**Key Components:**
- **Services (9+):**
  - `SkillService` - Skill domain operations
  - `AssessmentService` - Assessment workflow management
  - `QuestionService` - Question handling (CRUD, validation)
  - `DashboardService` - Analytics and reporting
  - `AuthService` - Authentication logic
  - `AIService` - AI integration (Claude API)
  - `SystemEnumService` - Dynamic enum management
  - And others for domains, subcategories, levels, etc.

- **DTOs:** Data Transfer Objects for API contracts
  - `CreateSkillDto`, `UpdateSkillDto`, `SkillDto`
  - `CreateAssessmentDto`, `AssessmentResponseDto`
  - `QuestionDto`, `QuestionOptionDto`
  - Similar patterns for all entities

- **Validators:** FluentValidation rules
  - Input validation for all DTOs
  - Business rule validation
  - Custom validation logic

- **Mappings:** AutoMapper profiles
  - Entity ↔ DTO conversions
  - Nested object mapping
  - Custom mapping logic

**Technologies:** Dependency Injection, Repository Pattern, AutoMapper, FluentValidation

#### 3. Domain Layer (SkillMatrix.Domain)
**Responsibility:** Core business logic, entity definitions, business rules

**Key Entities (50+):**

**Taxonomy (7 entities):**
- `SkillDomain` - Top-level skill categories
- `SkillSubcategory` - Mid-level grouping
- `Skill` - Individual skills with SFIA levels
- `SkillLevelDefinition` - Behavioral criteria per level
- `SkillRelationship` - Skill dependencies
- `ProficiencyLevelDefinition` - SFIA level (1-7) definitions

**Organization (9 entities):**
- `Team` - Department/team structure
- `JobRole` - Position definitions with career ladder
- `RoleSkillRequirement` - Required skills per role
- `Employee` - User profiles with employment info
- `EmployeeSkill` - Current skill state (multi-source)
- `EmployeeSkillHistory` - Skill progression tracking
- `Project` - Project definitions
- `ProjectSkillRequirement` - Skills needed for projects
- `ProjectAssignment` - Employee-project mapping

**Assessment (6 entities):**
- `TestTemplate` - Reusable test definitions
- `TestSection` - Test sections with ordering
- `Question` - Test questions with multiple types
- `QuestionOption` - Multiple choice options
- `Assessment` - Assessment sessions (in-progress, completed)
- `AssessmentSkillResult` - Results per skill
- `AssessmentResponse` - Individual question answers

**Learning (6 entities):**
- `SkillGap` - Individual gaps (current vs. required)
- `TeamSkillGap` - Team-level gaps
- `LearningResource` - Training materials
- `LearningResourceSkill` - Resource-skill mapping
- `EmployeeLearningPath` - Personalized learning paths
- `LearningPathItem` - Individual path items

**Configuration (1 entity):**
- `SystemEnumValue` - Configurable enumeration values

**Common Base Classes:**
- `BaseEntity` - Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- `VersionedEntity` - Versioning support (EffectiveFrom, EffectiveTo, IsCurrent)
- `SoftDeleteEntity` - Soft delete support (IsDeleted)

**Enumerations:**
- `ProficiencyLevel` (0-7) - SFIA levels (hard-coded)
- `AssessmentType` - Quiz, CodingTest, CaseStudy, etc.
- `QuestionType` - MultipleChoice, ShortAnswer, CodingChallenge, etc.
- `DifficultyLevel` - Easy, Medium, Hard, Expert
- `UserRole` - Employee, Manager, Admin, TeamLead
- And 8+ more configurable enums

**Technologies:** Entity Framework Core, Domain-Driven Design patterns

#### 4. Infrastructure Layer (SkillMatrix.Infrastructure)
**Responsibility:** Data persistence, external service integration

**Key Components:**
- **DbContext:**
  - `SkillMatrixDbContext` - EF Core context
  - Entity configurations with fluent API
  - Global query filters for soft delete
  - Relationship mappings

- **Repositories:**
  - Generic `Repository<T>` pattern
  - Specialized repositories for complex queries
  - Unit of work pattern for transactions

- **Migrations:**
  - Database schema versioning
  - Seed data migration
  - PostgreSQL-specific configurations

- **Seeders:**
  - Initial data population
  - Default skill taxonomies
  - Test users and assessments

- **Services:**
  - AI service wrapper (Claude API)
  - Email service (future)
  - External integrations

**Technologies:** Entity Framework Core, PostgreSQL, Repository Pattern, Dependency Injection

## Frontend Architecture

### Directory Structure

```
web/src/
├── pages/                    # Page-level components
│   ├── auth/
│   │   ├── Login.tsx
│   │   └── Register.tsx
│   ├── dashboard/
│   │   ├── Dashboard.tsx
│   │   ├── SkillMatrix.tsx
│   │   └── EmployeeSkills.tsx
│   ├── taxonomy/
│   │   ├── Domains.tsx
│   │   ├── Subcategories.tsx
│   │   ├── Skills.tsx
│   │   └── Levels.tsx
│   ├── assessment/
│   │   ├── AvailableTests.tsx
│   │   ├── TakeTest.tsx
│   │   └── TestResult.tsx
│   ├── templates/
│   │   ├── TestTemplateList.tsx
│   │   ├── TestTemplateDetail.tsx
│   │   └── TemplateEditor.tsx
│   └── admin/
│       └── SystemEnums.tsx
│
├── components/               # Reusable components
│   ├── Layout/
│   ├── Forms/
│   ├── Tables/
│   ├── Cards/
│   ├── Modals/
│   └── Common/
│
├── api/                      # API integration layer
│   ├── client.ts             # Axios instance with base config
│   ├── auth.ts               # Authentication endpoints
│   ├── skills.ts             # Skill endpoints
│   ├── assessments.ts        # Assessment endpoints
│   ├── templates.ts          # Template endpoints
│   ├── dashboard.ts          # Dashboard endpoints
│   └── enums.ts              # Enum endpoints
│
├── hooks/                    # Custom React hooks
│   ├── useAuth.ts            # Authentication state
│   ├── useFetch.ts           # Data fetching
│   ├── useForm.ts            # Form state management
│   └── Others
│
├── types/                    # TypeScript interfaces
│   ├── auth.ts
│   ├── skill.ts
│   ├── assessment.ts
│   ├── common.ts
│   └── api.ts
│
├── utils/                    # Utility functions
│   ├── formatters.ts
│   ├── validators.ts
│   ├── helpers.ts
│   └── constants.ts
│
├── styles/                   # Global styles
│   ├── globals.css
│   ├── variables.css
│   └── theme.ts
│
├── App.tsx                   # Main app component with routing
├── main.tsx                  # Entry point
└── index.html               # HTML template
```

### Technology Stack

- **React 19.2.0** - UI framework with latest features
- **TypeScript 5.9.3** - Type-safe development
- **Vite 7.2.4** - Fast build tooling
- **Ant Design 6.2.1** - Enterprise UI component library
- **React Query 5.90.19** - Server state management
- **Axios** - HTTP client for API calls
- **TailwindCSS** - Utility-first styling (if used)

### Page Overview (15+ Pages)

| Page | Path | Purpose | Status |
|------|------|---------|--------|
| Login | `/login` | User authentication | Complete |
| Dashboard | `/dashboard` | System overview, metrics | Complete |
| Test List | `/tests` | Available assessments | Complete |
| Take Test | `/tests/:id/take` | Assessment execution | Complete |
| Test Result | `/tests/:id/result` | Assessment results | Complete |
| Skill Domains | `/taxonomy/domains` | Domain management | Complete |
| Subcategories | `/taxonomy/subcategories` | Subcategory management | Complete |
| Skills | `/taxonomy/skills` | Skill CRUD | Complete |
| Proficiency Levels | `/taxonomy/levels` | Level definitions | Complete |
| Test Templates | `/templates` | Template list | Complete |
| Template Detail | `/templates/:id` | Template editor | Complete |
| System Enums | `/admin/enums` | Dynamic enum config | Complete |
| Job Roles | `/roles` | Role management | Pending |
| Learning Paths | `/learning` | Learning path tracking | Pending |
| Employee Profile | `/employees/:id` | Employee details | Pending |

## Data Flow

### Assessment Workflow

```
User (Frontend)
    ↓
TakeTest.tsx (page)
    ↓ (calls)
assessments.ts (API client)
    ↓ (HTTP request)
AssessmentsController (API)
    ↓ (calls)
AssessmentService (business logic)
    ↓ (calls)
Repository (data access)
    ↓
PostgreSQL Database
    ↓
Result ← Response ← DTO ← Entity
```

### Skill Query Flow

```
Dashboard.tsx
    ↓
useSuspenseQuery (React Query)
    ↓
skills.ts (API client)
    ↓
SkillsController → SkillService
    ↓
Repository + LINQ queries
    ↓
PostgreSQL → Entity → DTO
    ↓
UI Components (display data)
```

## Key Patterns

### Backend Patterns

1. **Clean Architecture**
   - Clear separation of concerns
   - Dependency inversion principle
   - Testable layers

2. **Repository Pattern**
   - Abstracted data access
   - Easy to mock for testing
   - Centralized query logic

3. **Service Layer**
   - Business logic encapsulation
   - DTOs for API contracts
   - Transaction management

4. **Soft Delete**
   - Data preservation
   - Global query filters
   - Historical tracking

5. **Audit Trail**
   - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
   - Track all changes
   - Compliance requirements

### Frontend Patterns

1. **Page-based Architecture**
   - Colocated components
   - Feature-based organization
   - Easy navigation

2. **API Client Layer**
   - Centralized API calls
   - Reusable endpoints
   - Easy testing

3. **React Query Integration**
   - Efficient data fetching
   - Automatic caching
   - Background synchronization

4. **TypeScript Safety**
   - Strong typing throughout
   - Compile-time error detection
   - Better IDE support

5. **Component Composition**
   - Reusable components
   - Props-based configuration
   - Clear responsibilities

## Entry Points

### Backend Entry Point
- **File:** `src/SkillMatrix.Api/Program.cs`
- **Configures:** Dependency injection, middleware, database
- **Starts:** ASP.NET Core host on port 5001

### Frontend Entry Point
- **File:** `web/src/main.tsx`
- **Mounts:** React app to DOM
- **Starts:** App.tsx with routing

## Build & Deployment

### Backend Build
```bash
dotnet build                    # Compile
dotnet test                     # Run tests
dotnet publish -c Release       # Build for deployment
```

### Frontend Build
```bash
npm install                     # Dependencies
npm run dev                     # Dev server
npm run build                   # Production build
npm run preview                 # Preview build
```

### Automated Build
```bash
./publish.bat                   # Complete build automation
```

## Development Dependencies

### Backend
- Entity Framework Core 9 (ORM)
- Asp.NetCore (Web framework)
- PostgreSQL driver
- AutoMapper (DTO mapping)
- FluentValidation (input validation)
- Swagger/Swashbuckle (API docs)

### Frontend
- React 19.2.0
- TypeScript 5.9.3
- Ant Design 6.2.1
- React Query 5.90.19
- Vite 7.2.4
- Axios
- Others (see package.json)

## Performance Considerations

1. **Database:** Connection pooling, query optimization, indexes
2. **Frontend:** Code splitting, lazy loading, image optimization
3. **API:** Pagination, filtering, caching strategies
4. **Build:** Tree-shaking, minification, source maps

## Security Measures

1. **Authentication:** Password-based with extensible JWT support
2. **Authorization:** Role-based access control (RBAC)
3. **Input Validation:** FluentValidation for server-side
4. **Data Protection:** Soft delete, audit trails
5. **API Security:** HTTPS/TLS, CORS configuration

---

**Document Version:** 1.0
**Last Updated:** 2026-01-23
