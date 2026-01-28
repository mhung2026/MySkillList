using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Application.Services;
using SkillMatrix.Application.Services.AI;
using SkillMatrix.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Skill Matrix API", Version = "v1" });
});

// Database - Connection string from appsettings.json or environment variable
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Please configure it in appsettings.json or environment variables.");

builder.Services.AddDbContext<SkillMatrixDbContext>(options =>
    options.UseNpgsql(connectionString));

// Taxonomy Services
builder.Services.AddScoped<ISkillDomainService, SkillDomainService>();
builder.Services.AddScoped<ISkillSubcategoryService, SkillSubcategoryService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IProficiencyLevelDefinitionService, ProficiencyLevelDefinitionService>();

// AI Service Configuration
builder.Services.Configure<AiServiceOptions>(
    builder.Configuration.GetSection(AiServiceOptions.SectionName));

var aiServiceOptions = builder.Configuration
    .GetSection(AiServiceOptions.SectionName)
    .Get<AiServiceOptions>() ?? new AiServiceOptions();

// AI Question Generator Service - Conditional based on UseMock
if (aiServiceOptions.UseMock)
{
    builder.Services.AddScoped<IAiQuestionGeneratorService, MockAiQuestionGeneratorService>();
}
else
{
    // Register HttpClient for Python AI service
    builder.Services.AddHttpClient<IAiQuestionGeneratorService, PythonAiQuestionGeneratorService>(client =>
    {
        client.BaseAddress = new Uri(aiServiceOptions.BaseUrl ?? "http://localhost:8002");
        client.Timeout = TimeSpan.FromSeconds(aiServiceOptions.TimeoutSeconds);
    });
}

// AI Skill Analyzer Service - Conditional based on UseMock
if (aiServiceOptions.UseMock)
{
    builder.Services.AddScoped<IAiSkillAnalyzerService, MockAiSkillAnalyzerService>();
}
else
{
    // Register HttpClient for Python AI skill analyzer
    builder.Services.AddHttpClient<IAiSkillAnalyzerService, PythonAiSkillAnalyzerService>(client =>
    {
        client.BaseAddress = new Uri(aiServiceOptions.BaseUrl ?? "http://localhost:8002");
        client.Timeout = TimeSpan.FromSeconds(aiServiceOptions.TimeoutSeconds);
    });
}

// AI Assessment Evaluator Service
builder.Services.AddHttpClient<IAiAssessmentEvaluatorService, PythonAiAssessmentEvaluatorService>(client =>
{
    client.BaseAddress = new Uri(aiServiceOptions.BaseUrl ?? "http://localhost:8002");
    client.Timeout = TimeSpan.FromSeconds(120); // Longer timeout for assessment evaluation
});

// Test/Assessment Services
builder.Services.AddScoped<ITestTemplateService, TestTemplateService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// Background service to auto-submit expired assessments
builder.Services.AddHostedService<AssessmentAutoSubmitService>();

// Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Dashboard Services
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Employee Profile Services
builder.Services.AddHttpClient<IAiLearningPathService, AiLearningPathService>(client =>
{
    client.BaseAddress = new Uri(aiServiceOptions.BaseUrl ?? "http://localhost:8002");
    client.Timeout = TimeSpan.FromSeconds(aiServiceOptions.TimeoutSeconds);
});
builder.Services.AddScoped<SkillMatrix.Infrastructure.Repositories.ICourseraCourseRepository,
    SkillMatrix.Infrastructure.Repositories.CourseraCourseRepository>();
builder.Services.AddScoped<IEmployeeProfileService, EmployeeProfileService>();

// Configuration Services
builder.Services.AddScoped<SystemEnumService>();

// Database Seeder
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<IDatabaseSeederService, DatabaseSeederService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:5176",
                "http://localhost:5177",
                "http://localhost:5178",
                "http://localhost:3000",
                "http://localhost",
                "https://myskilllist-ngeteam-ad.allianceitsc.com",
                "http://myskilllist-ngeteam-ad.allianceitsc.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Run pending SQL migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SkillMatrixDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Checking for pending SQL migrations...");

        var migrationsPath = Path.Combine(builder.Environment.ContentRootPath, "..", "SkillMatrix.Infrastructure", "Persistence", "Migrations");
        var sqlFiles = new[]
        {
            "20260127_AddAiLearningPathFields.sql",
            "20260127_AddLearningRecommendations.sql"
        };

        foreach (var sqlFile in sqlFiles)
        {
            var filePath = Path.Combine(migrationsPath, sqlFile);
            if (File.Exists(filePath))
            {
                logger.LogInformation("Applying migration: {SqlFile}", sqlFile);
                var sql = await File.ReadAllTextAsync(filePath);
                await context.Database.ExecuteSqlRawAsync(sql);
                logger.LogInformation("Successfully applied: {SqlFile}", sqlFile);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Migration may have already been applied or encountered an error");
    }
}

// Seed default data on startup
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.SeedDefaultUsersAsync();

    var levelDefinitionService = scope.ServiceProvider.GetRequiredService<IProficiencyLevelDefinitionService>();
    await levelDefinitionService.SeedDefaultLevelsAsync();

    var systemEnumService = scope.ServiceProvider.GetRequiredService<SystemEnumService>();
    await systemEnumService.SeedDefaultValuesAsync();
}

// Configure the HTTP request pipeline
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
