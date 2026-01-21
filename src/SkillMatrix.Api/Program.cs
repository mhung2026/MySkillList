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

// AI Services (Mock - sẽ replace bằng real AI service sau)
builder.Services.AddScoped<IAiQuestionGeneratorService, MockAiQuestionGeneratorService>();
builder.Services.AddScoped<IAiSkillAnalyzerService, MockAiSkillAnalyzerService>();

// Test/Assessment Services
builder.Services.AddScoped<ITestTemplateService, TestTemplateService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Dashboard Services
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configuration Services
builder.Services.AddScoped<SystemEnumService>();

// Database Seeder
builder.Services.AddScoped<DatabaseSeeder>();

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
