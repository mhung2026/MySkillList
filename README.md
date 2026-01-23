# SkillMatrix - Employee Skill Management System

A comprehensive platform for competency assessment, skill gap analysis, and learning path recommendations powered by SFIA 9 framework and AI.

## Overview

SkillMatrix helps organizations manage employee skills, conduct assessments, and recommend personalized learning paths. The system integrates SFIA 9 proficiency levels (1-7) for standardized skill evaluation and AI capabilities for automatic question generation and gap analysis.

**Current Status:** Core features stable. Extended features (Job Roles, Learning Paths, Reports) in development.

## Key Features

- **Skill Taxonomy Management** - SFIA 9 based skill classification with domains, subcategories, and proficiency levels
- **Assessment Engine** - Multiple assessment types: Quiz, Coding Tests, Case Studies, Situational Judgment, Role-Based Tests
- **Test Templates** - Reusable test templates with sections and question management
- **AI Question Generation** - Automatic test question generation with AI grading and explanations
- **Employee Skill Tracking** - Multi-source skill assessment (Self, Manager, Test-validated)
- **Dashboard & Analytics** - Skill matrix, team overview, performance metrics
- **Dynamic Configuration** - Admin-configurable enums for business customization
- **RBAC** - Role-based access control (Admin, Manager, Employee)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend API** | .NET 9.0, ASP.NET Core, Entity Framework Core 9 |
| **Database** | PostgreSQL |
| **Frontend** | React 19.2.0, TypeScript 5.9.3, Vite 7.2.4 |
| **UI Components** | Ant Design 6.2.1 |
| **State Management** | React Query (TanStack Query) 5.90.19 |
| **AI Integration** | Claude AI (mock service ready) |

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+
- PostgreSQL 14+

### Backend Setup

```bash
cd src/SkillMatrix.Api

# Install dependencies
dotnet restore

# Apply migrations
dotnet ef database update

# Run server
dotnet run
```

API available at: `https://localhost:5001`

### Frontend Setup

```bash
cd web

# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build
```

Frontend available at: `http://localhost:5173`

### Test Credentials

- **Email:** admin@skillmatrix.com
- **Password:** admin123

## Project Structure

```
SkillMatrix/
├── src/                           # Backend (.NET)
│   ├── SkillMatrix.Api/           # ASP.NET Core API
│   ├── SkillMatrix.Application/   # Business logic & services
│   ├── SkillMatrix.Domain/        # Domain models & entities
│   └── SkillMatrix.Infrastructure/# Database & external integrations
├── web/                           # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── pages/                 # Page components
│   │   ├── components/            # Reusable components
│   │   ├── api/                   # API integration layer
│   │   ├── hooks/                 # Custom React hooks
│   │   └── types/                 # TypeScript types
│   └── dist/                      # Production build output
├── docs/                          # Documentation
│   ├── README.md                  # This file
│   ├── SRS.md                     # Requirements specification
│   ├── database-design.md         # Database schema
│   ├── project-overview-pdr.md    # PDR & product vision
│   ├── code-standards.md          # Coding conventions
│   ├── system-architecture.md     # Architecture overview
│   └── project-roadmap.md         # Development roadmap
├── tools/                         # Utility scripts & tools
└── DEPLOY_GUIDE.md               # IIS deployment guide
```

## Core Concepts

### SFIA Proficiency Levels (1-7)

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

### Assessment Types

- **Quiz** - Multiple choice, true/false, short answer questions
- **Coding Test** - Code challenges with test cases
- **Case Study** - Scenario-based questions
- **Situational Judgment Test (SJT)** - Behavioral effectiveness assessment
- **Role-Based Test** - Role-specific skill evaluation

### Question Types

MultipleChoice, MultipleAnswer, TrueFalse, ShortAnswer, LongAnswer, CodingChallenge, Scenario, SituationalJudgment

## API Overview

### Core Endpoints

**Taxonomy:**
- `GET/POST/PUT/DELETE /api/skills` - Skill management
- `GET/POST/PUT/DELETE /api/skilldomains` - Domain management
- `GET/POST/PUT/DELETE /api/leveldefinitions` - Proficiency level definitions

**Assessment:**
- `GET/POST/PUT/DELETE /api/testtemplates` - Test template management
- `GET/POST/PUT/DELETE /api/questions` - Question management
- `POST /api/questions/generate-ai` - AI question generation
- `POST /api/assessments/start` - Start assessment
- `GET/POST /api/assessments/{id}` - Assessment workflow

**Configuration:**
- `GET/POST/PUT/DELETE /api/systemenums` - Dynamic enum management

**Dashboard:**
- `GET /api/dashboard/overview` - System statistics
- `GET /api/dashboard/employees/skills` - Skill overview

Full API documentation: `https://localhost:5001/swagger`

## Development Guidelines

- **Architecture:** Clean Architecture (API, Application, Domain, Infrastructure layers)
- **Database:** Entity Framework Core with migrations
- **Soft Delete:** All entities support soft delete via `IsDeleted` flag
- **Audit Trail:** Entities tracked with `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- **Versioning:** Key entities (Skills, Roles) support versioning with `EffectiveFrom`, `EffectiveTo`
- **CORS:** Configured for cross-origin requests between frontend and API

See `docs/code-standards.md` for detailed coding conventions.

## Deployment

### Development
```bash
dotnet run  # Backend
npm run dev # Frontend
```

### Production
See `DEPLOY_GUIDE.md` for IIS deployment instructions.

Quick publish:
```bash
./publish.bat
```

## Documentation Files

- **`docs/project-overview-pdr.md`** - Product vision, requirements, roadmap
- **`docs/codebase-summary.md`** - Code organization and structure
- **`docs/code-standards.md`** - Architecture patterns, conventions, best practices
- **`docs/system-architecture.md`** - Technical architecture overview
- **`docs/database-design.md`** - Database schema and relationships
- **`docs/SRS.md`** - Detailed requirements specification
- **`DEPLOY_GUIDE.md`** - IIS deployment guide

## Support & Resources

- **Issues:** Report via Azure DevOps
- **Swagger API Docs:** `/swagger` endpoint
- **Database Design:** `docs/database-design.md`
- **SRS Document:** `docs/SRS.md`

## License

Internal project - All rights reserved.
