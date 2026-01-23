# System Architecture

## Architecture Overview

SkillMatrix implements **Clean Architecture** with separation between API, Application, Domain, and Infrastructure layers. This enables testability, maintainability, and scalability.

```
┌────────────────────────────────────────────────────────────┐
│                    CLIENT LAYER                             │
│          React 19 Frontend (TypeScript/Vite)               │
│        ├─ Pages (15+)                                      │
│        ├─ Components (Reusable)                            │
│        ├─ API Client Layer (Axios)                         │
│        └─ State Management (React Query)                   │
└────────────────────────────────────────────────────────────┘
                            ↕ HTTPS
┌────────────────────────────────────────────────────────────┐
│                    API LAYER                                │
│         ASP.NET Core 9 Controllers (13)                    │
│        ├─ Authentication Endpoints                         │
│        ├─ Skill Management                                 │
│        ├─ Assessment Workflow                              │
│        ├─ Dashboard/Analytics                              │
│        ├─ Configuration (Enums)                            │
│        └─ CORS / Middleware                                │
└────────────────────────────────────────────────────────────┘
                            ↕
┌────────────────────────────────────────────────────────────┐
│                APPLICATION LAYER                            │
│            Services (9+) & Business Logic                  │
│        ├─ SkillService                                     │
│        ├─ AssessmentService                                │
│        ├─ QuestionService                                  │
│        ├─ DashboardService                                 │
│        ├─ AIService (Claude Integration)                   │
│        ├─ AuthService                                      │
│        ├─ SystemEnumService                                │
│        └─ DTOs & Validators                                │
└────────────────────────────────────────────────────────────┘
                            ↕
┌────────────────────────────────────────────────────────────┐
│                  DOMAIN LAYER                               │
│              Entities (50+) & Business Rules               │
│        ├─ Taxonomy (Skills, Domains, Levels)              │
│        ├─ Organization (Employees, Roles, Teams)          │
│        ├─ Assessment (Tests, Questions)                   │
│        ├─ Learning (Paths, Resources, Gaps)               │
│        └─ Configuration (System Enums)                    │
└────────────────────────────────────────────────────────────┘
                            ↕
┌────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE LAYER                           │
│         Data Access & External Services                    │
│        ├─ DbContext (EF Core 9)                            │
│        ├─ Repositories & Unit of Work                      │
│        ├─ PostgreSQL Adapter                               │
│        ├─ AI Service Integration                           │
│        └─ Seeders                                          │
└────────────────────────────────────────────────────────────┘
                            ↕ JDBC/PostgreSQL Driver
┌────────────────────────────────────────────────────────────┐
│                  PERSISTENCE LAYER                          │
│         PostgreSQL Database (50+ Tables)                   │
│        ├─ Skill Taxonomy Schema                            │
│        ├─ Organization Schema                              │
│        ├─ Assessment Schema                                │
│        ├─ Learning Schema                                  │
│        └─ Configuration Schema                             │
└────────────────────────────────────────────────────────────┘
```

## Technology Stack

### Backend

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Framework** | ASP.NET Core | 9.0 | Web API framework |
| **Language** | C# | 12 | Backend language |
| **ORM** | Entity Framework Core | 9.0 | Database abstraction |
| **Database** | PostgreSQL | 14+ | Relational database |
| **Logging** | Serilog | Latest | Structured logging |
| **Validation** | FluentValidation | Latest | Input validation |
| **Mapping** | AutoMapper | Latest | DTO mapping |
| **API Docs** | Swagger/Swashbuckle | Latest | API documentation |
| **AI Integration** | Claude API | Latest | Question generation |

### Frontend

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Framework** | React | 19.2.0 | UI framework |
| **Language** | TypeScript | 5.9.3 | Type-safe development |
| **Build Tool** | Vite | 7.2.4 | Fast bundler |
| **UI Library** | Ant Design | 6.2.1 | Component library |
| **State Mgmt** | React Query | 5.90.19 | Server state |
| **HTTP Client** | Axios | Latest | API requests |
| **Styling** | Ant Design Theme | Latest | Component styling |

### Deployment

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Web Server** | IIS 10.0+ | Production hosting |
| **OS** | Windows Server 2019+ | Server OS |
| **Runtime** | .NET 9 Hosting Bundle | ASP.NET Core runtime |
| **Build** | PowerShell/Batch | Automation scripts |

## Data Flow Architecture

### Assessment Workflow Flow

```
┌─────────────┐
│   Employee  │
│   Takes     │
│   Test      │
└──────┬──────┘
       │
       ↓ Click "Start Assessment"
┌──────────────────────────────────┐
│ Frontend (TakeTest.tsx)          │
│ - Load test template             │
│ - Display questions              │
│ - Capture answers                │
└──────────────────┬───────────────┘
       │
       ↓ HTTP POST /api/assessments/answer
┌──────────────────────────────────┐
│ AssessmentsController            │
│ - Route request                  │
│ - Validate input                 │
└──────────────────┬───────────────┘
       │
       ↓ Call service
┌──────────────────────────────────┐
│ AssessmentService                │
│ - Process answer                 │
│ - Validate logic                 │
│ - Update assessment state        │
└──────────────────┬───────────────┘
       │
       ├─→ For AI questions:
       │   ↓
       │   ┌──────────────────────┐
       │   │ AIService            │
       │   │ - Grade answer       │
       │   │ - Generate feedback  │
       │   └──────────────────┬───┘
       │   │
       │   ↓
       │   ┌──────────────────────┐
       │   │ Claude API           │
       │   │ - Grade logic        │
       │   │ - Explanation        │
       │   └──────────────────┬───┘
       │   │
       │   ↓ Response
       │   └──────────────────┬───┘
       │
       └─→ Repository.SaveAsync()
           │
           ↓ EF Core
       ┌──────────────────────────────┐
       │ PostgreSQL                   │
       │ - Save AssessmentResponse    │
       │ - Update Assessment status   │
       └──────────────────────────────┘
```

### Skill Query Flow

```
┌─────────────┐
│  Dashboard  │
│  Page Load  │
└──────┬──────┘
       │
       ↓ useSuspenseQuery('skills')
┌──────────────────────────────────┐
│ React Query                      │
│ - Check cache                    │
│ - Check validity                 │
└──────────────────┬───────────────┘
       │
       ├─ If cached: Return cached data
       │
       └─ If not cached or stale:
           │
           ↓ Call skills.getAll()
       ┌──────────────────────────┐
       │ API Client (Axios)       │
       │ - Add auth token         │
       │ - HTTP GET /api/skills   │
       └────────────┬─────────────┘
           │
           ↓ HTTP
       ┌──────────────────────────────┐
       │ SkillsController             │
       │ - Validate auth              │
       │ - Route request              │
       └────────────┬─────────────────┘
           │
           ↓ Call service
       ┌──────────────────────────────┐
       │ SkillService.GetAllAsync()   │
       │ - Business logic             │
       └────────────┬─────────────────┘
           │
           ↓ Repository.GetAllAsync()
       ┌──────────────────────────────┐
       │ EF Core LINQ Query           │
       │ - Apply filters              │
       │ - Include relations          │
       │ - Execute on DB              │
       └────────────┬─────────────────┘
           │
           ↓ PostgreSQL
       ┌──────────────────────────────┐
       │ SELECT * FROM skills         │
       │ WHERE IsDeleted = false       │
       │ ORDER BY name                │
       └────────────┬─────────────────┘
           │
           ↓ Result set
       ┌──────────────────────────────┐
       │ Map to SkillDto              │
       │ AutoMapper conversion        │
       └────────────┬─────────────────┘
           │
           ↓ JSON response
       ┌──────────────────────────────┐
       │ React Query                  │
       │ - Cache result               │
       │ - Set query status           │
       │ - Rerender component         │
       └────────────┬─────────────────┘
           │
           ↓ Re-render
       ┌──────────────────────────────┐
       │ Dashboard Component          │
       │ - Display skills in table    │
       └──────────────────────────────┘
```

## Layer Responsibilities

### API Layer

**Responsibility:** HTTP interface, request routing, middleware orchestration

**Components:**
- 13 REST Controllers (Auth, Skills, Tests, Assessments, etc.)
- Request/Response mapping
- HTTP status codes
- Swagger/OpenAPI documentation
- CORS configuration
- Authentication middleware

**Key Files:**
- `src/SkillMatrix.Api/Program.cs` - DI configuration
- `src/SkillMatrix.Api/Controllers/*Controller.cs` - Endpoints

**Contract:** Accepts HTTP requests, returns JSON responses

### Application Layer

**Responsibility:** Business logic, orchestration, data transformation

**Components:**
- Services (SkillService, AssessmentService, etc.)
- DTOs for data transfer
- Input validators
- AutoMapper profiles
- Exception handling logic
- Transaction management

**Key Files:**
- `src/SkillMatrix.Application/Services/*.cs`
- `src/SkillMatrix.Application/DTOs/*.cs`
- `src/SkillMatrix.Application/Validators/*.cs`

**Contract:** Enforces business rules, transforms entities to DTOs

### Domain Layer

**Responsibility:** Core business logic, entity definitions, domain rules

**Components:**
- 50+ Domain entities
- Business value objects
- Domain enumerations
- Interface contracts
- Base entity classes
- Domain services (rare)

**Key Files:**
- `src/SkillMatrix.Domain/Entities/*.cs`
- `src/SkillMatrix.Domain/Enums/*.cs`
- `src/SkillMatrix.Domain/Interfaces/*.cs`

**Contract:** Pure business logic, framework-agnostic, highly reusable

### Infrastructure Layer

**Responsibility:** Data persistence, external integrations, technical details

**Components:**
- DbContext and migrations
- Repository implementations
- Database adapters
- External service clients (AI, email, etc.)
- Seeding logic
- Configuration management

**Key Files:**
- `src/SkillMatrix.Infrastructure/Data/SkillMatrixDbContext.cs`
- `src/SkillMatrix.Infrastructure/Repositories/*.cs`
- `src/SkillMatrix.Infrastructure/Services/*.cs`

**Contract:** Implements interfaces, handles persistence and I/O

## Frontend Architecture

### Component Hierarchy

```
App.tsx
├── Layout Component
│   ├── Header (Navigation, Logo, User Menu)
│   ├── Sidebar (Navigation Menu)
│   └── MainContent
│       └── Pages (via React Router)
│           ├── Dashboard Page
│           ├── Taxonomy Pages
│           ├── Assessment Pages
│           ├── Template Pages
│           └── Admin Pages
│
└── Providers
    ├── QueryClientProvider (React Query)
    ├── ThemeProvider (Ant Design)
    └── AuthProvider (Context)
```

### Page Layers

```
Page Component (e.g., Dashboard.tsx)
├── Data Fetching (useQuery/useSuspenseQuery)
├── State Management (useState, useReducer)
├── Event Handlers
└── Rendering
    ├── Layout Components
    │   ├── Cards
    │   ├── Tables
    │   └── Charts
    └── Business Components
        ├── SkillMatrix
        ├── AssessmentList
        └── EmployeeSkills
```

## Cross-Cutting Concerns

### Authentication & Authorization

```
User Login
    ↓
POST /api/auth/login
    ↓ Validate credentials
AuthService
    ↓ Generate JWT
    ↓
Return { token, user }
    ↓
Frontend stores in localStorage
    ↓
On every request: Authorization header with Bearer token
    ↓
AuthenticationMiddleware validates token
    ↓
Request proceeds with user context
```

### Logging

**Backend:**
- Serilog for structured logging
- Logged to console and files
- Log levels: Debug, Information, Warning, Error, Fatal
- Context includes UserId, RequestId, Timestamps

**Frontend:**
- Browser console (dev environment)
- Error boundaries for crash reporting
- Network request logging for debugging

### Error Handling

**Backend:**
```
1. Custom exceptions thrown in services
2. Global exception middleware catches
3. Maps to appropriate HTTP status code
4. Returns error response to client
5. Logs error with context
```

**Frontend:**
```
1. API client intercepts errors
2. React Query handles retries
3. Component renders error boundary
4. User sees friendly error message
5. Can retry manually
```

### CORS Configuration

**Allowed Origins:**
- `http://localhost:5173` - Local development
- `https://myskilllist-ngeteam-ad.allianceitsc.com` - Production frontend
- `http://localhost:3000` - Optional alternative

**Allowed Methods:** GET, POST, PUT, DELETE, PATCH, OPTIONS
**Allowed Headers:** Content-Type, Authorization, X-Requested-With

### Database Design Principles

1. **Relational Model:** PostgreSQL ACID compliance
2. **Soft Delete:** `IsDeleted` flag for data preservation
3. **Audit Trail:** CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
4. **Versioning:** Support for evolving entities (EffectiveFrom, EffectiveTo)
5. **Indexes:** On foreign keys and frequently queried fields
6. **Constraints:** Foreign keys, unique constraints, check constraints
7. **Normalization:** 3NF to reduce redundancy

## Integration Points

### External Integrations

**Claude AI API**
- Endpoint: `POST /api/questions/generate-ai`
- Purpose: AI question generation with context
- Error Handling: Graceful fallback to manual generation
- Rate Limiting: Implemented caching strategy

**Database**
- Provider: PostgreSQL
- Connection: Entity Framework Core
- Migrations: Code-first approach
- Backup: Configured at server level

**Frontend-Backend**
- Protocol: HTTPS REST API
- Format: JSON request/response
- Authentication: Bearer token in headers
- Error codes: Standard HTTP status codes

## Security Architecture

### Authentication Flow

```
1. User submits credentials (email, password)
2. Backend validates against user entity
3. Password verified with hashing algorithm
4. If valid: Generate JWT token with claims
5. Return token to frontend
6. Frontend stores in localStorage
7. Add to Authorization header on requests
8. Backend validates token on protected endpoints
```

### Authorization Flow

```
1. Token contains user role (Admin, Manager, Employee)
2. Controller decorated with [Authorize(Roles = "Admin")]
3. Token extracted from header
4. Role verified from token claims
5. If authorized: Proceed; If not: Return 403
6. Resource-level checks in service layer
```

### Data Protection

- **In Transit:** HTTPS/TLS encryption
- **In Rest:** Database-level encryption (if configured)
- **Soft Delete:** Prevents data loss, enables recovery
- **Audit Trail:** Track all changes
- **Input Validation:** Server-side validation prevents injection

## Performance Architecture

### Caching Strategy

**Backend Caching:**
- Database query result caching (short TTL)
- Redis (future enhancement)

**Frontend Caching:**
- React Query automatic caching
- LocalStorage for user preferences
- Session storage for temporary data

### Database Optimization

- Indexes on foreign keys
- Includes for related entities
- Pagination for large result sets
- Materialized views (if needed)

### API Optimization

- Response DTO fields (only needed data)
- Compression (gzip)
- Connection pooling
- Rate limiting (future)

### Frontend Optimization

- Code splitting by routes
- Lazy loading for components
- Image optimization
- CSS/JS minification

## Deployment Architecture

### Environment Configuration

**Development**
- Local PostgreSQL
- API on port 5001
- Frontend on port 5173
- Mock services

**Production (IIS)**
- Hosted PostgreSQL
- API site with HTTPS
- Frontend site with URL rewrite
- Real external services

### Build Pipeline

```
Source Code
    ↓
Git Commit
    ↓
Automated Build (publish.bat)
    ├─ Backend: dotnet publish -c Release
    ├─ Frontend: npm run build
    └─ Output: dist folders
    ↓
Copy to IIS Folders
    ├─ C:\inetpub\skillmatrix-api
    └─ C:\inetpub\skillmatrix-web
    ↓
Restart IIS
    ↓
Production Live
```

## Scalability Considerations

### Horizontal Scaling

**Backend:**
- Stateless API enables load balancing
- Multiple instances behind load balancer
- Shared database connection string

**Frontend:**
- Static files served by CDN
- Multiple server instances
- Client-side state management

### Database Scaling

- Connection pooling (implemented)
- Read replicas (future)
- Sharding strategy (future)
- Archive old data (future)

### Caching Strategy

- Output caching for static content
- Distributed cache (Redis) for shared state
- Query result caching

---

**Document Version:** 1.0
**Last Updated:** 2026-01-23
