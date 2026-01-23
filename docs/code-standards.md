# Code Standards & Best Practices

## Architecture Overview

SkillMatrix follows **Clean Architecture** with four distinct layers ensuring separation of concerns and maintainability.

```
┌─────────────────────────────────────────────┐
│           API Layer (Controllers)           │  <- HTTP Interface
├─────────────────────────────────────────────┤
│        Application Layer (Services)         │  <- Business Logic
├─────────────────────────────────────────────┤
│          Domain Layer (Entities)            │  <- Core Logic
├─────────────────────────────────────────────┤
│      Infrastructure Layer (Data Access)    │  <- Persistence
└─────────────────────────────────────────────┘
```

## Backend Standards

### 1. Naming Conventions

#### C# Naming
```csharp
// Classes: PascalCase
public class SkillService { }
public class CreateSkillDto { }

// Methods: PascalCase
public async Task<SkillDto> GetSkillByIdAsync(Guid id) { }

// Properties: PascalCase
public string Name { get; set; }

// Private fields: _camelCase
private readonly IRepository<Skill> _skillRepository;

// Constants: UPPER_SNAKE_CASE
private const string DEFAULT_LANGUAGE = "en";

// Parameters: camelCase
public void CreateSkill(string skillName, int level) { }

// Local variables: camelCase
var skillCount = skills.Count();
```

#### Entity Naming
- **Plural for collections:** `Skills`, `Assessments`, `Employees`
- **Singular for entities:** `Skill`, `Assessment`, `Employee`
- **Descriptive names:** `SkillLevelDefinition` not `SLD` or `LevelDef`

#### DTO Naming
```csharp
// Request DTOs
public class CreateSkillDto { }      // Create operations
public class UpdateSkillDto { }      // Update operations
public class SkillFilterDto { }      // Filtering/Search

// Response DTOs
public class SkillDto { }            // Standard response
public class SkillDetailDto { }      // Detailed response
public class SkillListItemDto { }    // List item response
```

#### Repository Naming
```csharp
// Interface
IRepository<Skill>
ISkillRepository
IAssessmentRepository

// Implementation
Repository<Skill>
SkillRepository
```

### 2. Entity Structure

#### Base Entity Pattern
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public abstract class SoftDeleteEntity : BaseEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public abstract class VersionedEntity : SoftDeleteEntity
{
    public int Version { get; set; } = 1
    public bool IsCurrent { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
```

#### Example Entity
```csharp
public class Skill : VersionedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;

    // Foreign keys
    public Guid SkillSubcategoryId { get; set; }

    // Navigation properties
    public virtual SkillSubcategory? SkillSubcategory { get; set; }
    public virtual ICollection<SkillLevelDefinition> LevelDefinitions { get; set; }
        = new List<SkillLevelDefinition>();
    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; }
        = new List<EmployeeSkill>();
}
```

### 3. Service Layer Pattern

```csharp
public interface ISkillService
{
    Task<SkillDto> GetByIdAsync(Guid id);
    Task<IEnumerable<SkillDto>> GetAllAsync();
    Task<SkillDto> CreateAsync(CreateSkillDto dto);
    Task<SkillDto> UpdateAsync(Guid id, UpdateSkillDto dto);
    Task DeleteAsync(Guid id);
}

public class SkillService : ISkillService
{
    private readonly IRepository<Skill> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<SkillService> _logger;

    public SkillService(
        IRepository<Skill> repository,
        IMapper mapper,
        ILogger<SkillService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SkillDto> GetByIdAsync(Guid id)
    {
        var skill = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Skill {id} not found");

        return _mapper.Map<SkillDto>(skill);
    }

    public async Task<SkillDto> CreateAsync(CreateSkillDto dto)
    {
        var skill = _mapper.Map<Skill>(dto);
        skill.Id = Guid.NewGuid();
        skill.CreatedAt = DateTime.UtcNow;

        var created = await _repository.AddAsync(skill);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Skill {SkillId} created", created.Id);
        return _mapper.Map<SkillDto>(created);
    }
}
```

### 4. Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(
        ISkillService skillService,
        ILogger<SkillsController> logger)
    {
        _skillService = skillService;
        _logger = logger;
    }

    /// <summary>
    /// Get skill by ID
    /// </summary>
    /// <param name="id">Skill ID</param>
    /// <returns>Skill details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var skill = await _skillService.GetByIdAsync(id);
        return Ok(skill);
    }

    /// <summary>
    /// Get all skills
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll()
    {
        var skills = await _skillService.GetAllAsync();
        return Ok(skills);
    }

    /// <summary>
    /// Create new skill
    /// </summary>
    /// <param name="dto">Create DTO</param>
    /// <returns>Created skill</returns>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateSkillDto dto)
    {
        var skill = await _skillService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = skill.Id }, skill);
    }

    /// <summary>
    /// Update skill
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSkillDto dto)
    {
        var skill = await _skillService.UpdateAsync(id, dto);
        return Ok(skill);
    }

    /// <summary>
    /// Delete skill
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _skillService.DeleteAsync(id);
        return NoContent();
    }
}
```

### 5. DTO Pattern

```csharp
// Request DTO
public class CreateSkillDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid SkillSubcategoryId { get; set; }
}

// Response DTO
public class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Code { get; set; }
    public SkillSubcategoryDto? Subcategory { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
```

### 6. Validation Pattern

```csharp
public class CreateSkillValidator : AbstractValidator<CreateSkillDto>
{
    private readonly IRepository<SkillSubcategory> _subcategoryRepo;

    public CreateSkillValidator(IRepository<SkillSubcategory> subcategoryRepo)
    {
        _subcategoryRepo = subcategoryRepo;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required and must be ≤ 100 characters");

        RuleFor(x => x.SkillSubcategoryId)
            .NotEmpty()
            .MustAsync(SubcategoryExists)
            .WithMessage("Subcategory not found");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must be ≤ 500 characters");
    }

    private async Task<bool> SubcategoryExists(Guid id, CancellationToken ct)
    {
        return await _subcategoryRepo.AnyAsync(s => s.Id == id);
    }
}
```

### 7. Dependency Injection Setup

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<SkillMatrixDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add Services
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAIService, AIService>();

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateSkillValidator>();

// Add MediatR (if using CQRS)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://myskilllist-ngeteam-ad.allianceitsc.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Add Swagger
builder.Services.AddSwaggerGen();
```

### 8. Database Query Best Practices

```csharp
// ✅ Good: Explicit includes, filtering at DB
var skills = await _context.Skills
    .Where(s => !s.IsDeleted && s.SkillSubcategoryId == subcategoryId)
    .Include(s => s.SkillSubcategory)
    .Include(s => s.LevelDefinitions)
    .ToListAsync();

// ❌ Bad: Loading all then filtering in memory
var allSkills = await _context.Skills.ToListAsync();
var filtered = allSkills.Where(s => s.SkillSubcategoryId == subcategoryId).ToList();

// ✅ Good: Async all the way
public async Task<SkillDto> GetByIdAsync(Guid id)
{
    var skill = await _repository.GetByIdAsync(id);
    return _mapper.Map<SkillDto>(skill);
}

// ❌ Bad: Blocking calls
public SkillDto GetById(Guid id)
{
    var skill = _repository.GetByIdAsync(id).Result;  // Blocks!
    return _mapper.Map<SkillDto>(skill);
}
```

### 9. Error Handling Pattern

```csharp
public class CustomExceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }
        public ValidationException(Dictionary<string, string> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}

// Global exception handler middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        var response = new { error = exception?.Message };
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            NotFoundException => 404,
            ValidationException => 400,
            UnauthorizedException => 401,
            _ => 500
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});
```

### 10. Async/Await Guidelines

```csharp
// ✅ Use async all the way
public async Task<SkillDto> GetSkillAsync(Guid id)
{
    return await _skillService.GetByIdAsync(id);
}

// ✅ ConfigureAwait for library code
var result = await httpClient.GetAsync(url).ConfigureAwait(false);

// ❌ Avoid: Fire and forget
#pragma warning disable CS4014
_backgroundService.ProcessAsync();
#pragma warning restore CS4014

// ✅ Better: Track background tasks
_ = _backgroundService.ProcessAsync();

// ❌ Avoid: Sync-over-async
var result = _skillService.GetByIdAsync(id).GetAwaiter().GetResult();
```

## Frontend Standards

### 1. TypeScript Conventions

```typescript
// Interfaces: PascalCase with 'I' prefix (optional but common)
interface Skill {
  id: string;
  name: string;
  level: number;
}

// Types: PascalCase
type SkillLevel = 'beginner' | 'intermediate' | 'advanced';

// Enums: PascalCase
enum AssessmentStatus {
  Draft = 'draft',
  InProgress = 'inProgress',
  Completed = 'completed',
}

// Functions: camelCase
function formatSkillLevel(level: number): string {
  return `Level ${level}`;
}

// Constants: UPPER_SNAKE_CASE
const MAX_SKILL_NAME_LENGTH = 100;

// Variables: camelCase
const skillCount = 42;
```

### 2. React Component Pattern

```typescript
// Functional component with TypeScript
interface SkillListProps {
  skills: Skill[];
  onSkillSelect: (skill: Skill) => void;
  isLoading?: boolean;
}

export const SkillList: React.FC<SkillListProps> = ({
  skills,
  onSkillSelect,
  isLoading = false,
}) => {
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const handleSelect = (skill: Skill) => {
    setSelectedId(skill.id);
    onSkillSelect(skill);
  };

  if (isLoading) {
    return <Spin />;
  }

  return (
    <List
      dataSource={skills}
      renderItem={(skill) => (
        <List.Item
          onClick={() => handleSelect(skill)}
          style={{
            backgroundColor: selectedId === skill.id ? '#f0f0f0' : 'transparent',
            cursor: 'pointer',
          }}
        >
          {skill.name}
        </List.Item>
      )}
    />
  );
};
```

### 3. Custom Hooks Pattern

```typescript
// Custom hook for API calls
interface UseFetchOptions {
  skip?: boolean;
  refetchInterval?: number;
}

function useFetchSkill(skillId: string, options?: UseFetchOptions) {
  const { data, isLoading, error } = useQuery({
    queryKey: ['skill', skillId],
    queryFn: () => skillApi.getById(skillId),
    enabled: !options?.skip && !!skillId,
    refetchInterval: options?.refetchInterval,
  });

  return { skill: data, isLoading, error };
}

// Usage
const MyComponent: React.FC = () => {
  const { skill, isLoading, error } = useFetchSkill('123');

  if (isLoading) return <Spin />;
  if (error) return <Alert type="error" message="Failed to load skill" />;

  return <div>{skill?.name}</div>;
};
```

### 4. API Client Pattern

```typescript
// api/client.ts
import axios, { AxiosInstance } from 'axios';

const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:5001/api';

export const apiClient: AxiosInstance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// api/skills.ts
export const skillApi = {
  getById: (id: string) => apiClient.get<SkillDto>(`/skills/${id}`),
  getAll: () => apiClient.get<SkillDto[]>('/skills'),
  create: (dto: CreateSkillDto) => apiClient.post<SkillDto>('/skills', dto),
  update: (id: string, dto: UpdateSkillDto) =>
    apiClient.put<SkillDto>(`/skills/${id}`, dto),
  delete: (id: string) => apiClient.delete(`/skills/${id}`),
};
```

### 5. Form Handling

```typescript
// Using Ant Design Form
const SkillForm: React.FC<{ skillId?: string }> = ({ skillId }) => {
  const [form] = Form.useForm<CreateSkillDto>();
  const { mutate: createSkill, isPending } = useMutation({
    mutationFn: skillApi.create,
    onSuccess: () => {
      message.success('Skill created successfully');
      form.resetFields();
    },
  });

  const onFinish = async (values: CreateSkillDto) => {
    createSkill(values);
  };

  return (
    <Form form={form} onFinish={onFinish} layout="vertical">
      <Form.Item
        label="Name"
        name="name"
        rules={[
          { required: true, message: 'Please input skill name' },
          { max: 100, message: 'Name must be ≤ 100 characters' },
        ]}
      >
        <Input placeholder="Enter skill name" />
      </Form.Item>

      <Form.Item
        label="Description"
        name="description"
        rules={[{ max: 500 }]}
      >
        <TextArea rows={4} />
      </Form.Item>

      <Button type="primary" htmlType="submit" loading={isPending}>
        Create Skill
      </Button>
    </Form>
  );
};
```

## Code Review Guidelines

### Backend Review Checklist

- [ ] Follows Clean Architecture layers
- [ ] Proper naming conventions applied
- [ ] DTOs used for API contracts
- [ ] Validation at application layer
- [ ] Async/await throughout
- [ ] Exception handling present
- [ ] Database queries optimized (no N+1)
- [ ] Unit tests included
- [ ] XML documentation on public members
- [ ] No hardcoded values or magic numbers

### Frontend Review Checklist

- [ ] TypeScript strict mode enabled
- [ ] Proper component composition
- [ ] Props fully typed
- [ ] No prop drilling (use context if needed)
- [ ] Error states handled
- [ ] Loading states shown
- [ ] Accessibility attributes included (aria-*)
- [ ] Responsive design verified
- [ ] API errors handled gracefully
- [ ] Performance optimized (memo, useMemo, useCallback)

## Common Patterns

### Repository Pattern Example

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
```

### Unit of Work Pattern

```csharp
public interface IUnitOfWork
{
    IRepository<Skill> Skills { get; }
    IRepository<Assessment> Assessments { get; }
    Task<int> SaveChangesAsync();
}
```

### Soft Delete Query Filter

```csharp
// In DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Skill>()
        .HasQueryFilter(s => !s.IsDeleted);

    modelBuilder.Entity<Assessment>()
        .HasQueryFilter(a => !a.IsDeleted);
}
```

## Best Practices Summary

| Area | Practice |
|------|----------|
| **Architecture** | Clean Architecture with clear layer separation |
| **Naming** | Descriptive, consistent, follows language conventions |
| **Entities** | Inherit base classes, use navigation properties |
| **Services** | Business logic, dependency injection, async/await |
| **Controllers** | Thin, delegate to services, return proper status codes |
| **DTOs** | Separate request/response, validation attributes |
| **Database** | Queries optimized, includes explicit, pagination |
| **Error Handling** | Custom exceptions, global middleware, proper HTTP codes |
| **Frontend** | TypeScript strict, component composition, prop typing |
| **Testing** | Unit tests for services, integration tests for APIs |
| **Documentation** | XML comments, Swagger/OpenAPI, README files |

---

**Document Version:** 1.0
**Last Updated:** 2026-01-23
