using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillMatrix.Application.DTOs.Employee;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Organization;
using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public interface IDatabaseSeederService
{
    Task<SeedResultDto> SeedSampleDataAsync();
}

public class DatabaseSeederService : IDatabaseSeederService
{
    private readonly SkillMatrixDbContext _context;
    private readonly IEmployeeProfileService _employeeProfileService;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(
        SkillMatrixDbContext context,
        IEmployeeProfileService employeeProfileService,
        ILogger<DatabaseSeederService> logger)
    {
        _context = context;
        _employeeProfileService = employeeProfileService;
        _logger = logger;
    }

    public async Task<SeedResultDto> SeedSampleDataAsync()
    {
        var result = new SeedResultDto();

        try
        {
            _logger.LogInformation("Starting database seeding...");

            // 1. Seed Skills (if not exist)
            var skillsCreated = await SeedSkillsAsync();
            result.SkillsCreated = skillsCreated;

            // 2. Seed Job Roles with Skill Requirements
            var rolesCreated = await SeedJobRolesAsync();
            result.JobRolesCreated = rolesCreated;

            // 3. Seed Teams
            var teamsCreated = await SeedTeamsAsync();
            result.TeamsCreated = teamsCreated;

            // 4. Seed Employees with assigned roles and skills
            var employeesCreated = await SeedEmployeesAsync();
            result.EmployeesCreated = employeesCreated;

            // 5. Bulk recalculate gaps for all employees
            _logger.LogInformation("Triggering bulk gap recalculation...");
            var gapResult = await _employeeProfileService.BulkRecalculateGapsForAllEmployeesAsync();
            result.GapsCreated = gapResult.TotalGapsCreated;
            result.GapsUpdated = gapResult.TotalGapsUpdated;

            result.Success = true;
            result.Message = "Database seeded successfully";

            _logger.LogInformation(
                "Database seeding completed: {Skills} skills, {Roles} roles, {Teams} teams, {Employees} employees, {Gaps} gaps created",
                skillsCreated, rolesCreated, teamsCreated, employeesCreated, gapResult.TotalGapsCreated);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed database");
            result.Success = false;
            result.Message = $"Seeding failed: {ex.Message}";
            return result;
        }
    }

    private async Task<int> SeedSkillsAsync()
    {
        var existingSkills = await _context.Skills.CountAsync();
        if (existingSkills > 0)
        {
            _logger.LogInformation("Skills already exist, skipping...");
            return 0;
        }

        var skills = new List<Skill>
        {
            new() { Name = "C# Programming", Code = "CSHARP", Category = SkillCategory.Technical, Description = "C# and .NET development" },
            new() { Name = "ASP.NET Core", Code = "ASPNET", Category = SkillCategory.Technical, Description = "ASP.NET Core web development" },
            new() { Name = "Entity Framework", Code = "EF", Category = SkillCategory.Technical, Description = "Entity Framework Core ORM" },
            new() { Name = "SQL Server", Code = "SQL", Category = SkillCategory.Technical, Description = "Microsoft SQL Server" },
            new() { Name = "React", Code = "REACT", Category = SkillCategory.Technical, Description = "React.js frontend development" },
            new() { Name = "TypeScript", Code = "TS", Category = SkillCategory.Technical, Description = "TypeScript programming" },
            new() { Name = "API Design", Code = "API", Category = SkillCategory.Technical, Description = "RESTful API design" },
            new() { Name = "Git", Code = "GIT", Category = SkillCategory.Technical, Description = "Version control with Git" },
            new() { Name = "Agile/Scrum", Code = "AGILE", Category = SkillCategory.Professional, Description = "Agile methodology" },
            new() { Name = "Team Leadership", Code = "LEAD", Category = SkillCategory.Leadership, Description = "Leading development teams" },
            new() { Name = "Code Review", Code = "REVIEW", Category = SkillCategory.Technical, Description = "Code review practices" },
            new() { Name = "Testing", Code = "TEST", Category = SkillCategory.Technical, Description = "Unit and integration testing" },
            new() { Name = "CI/CD", Code = "CICD", Category = SkillCategory.Technical, Description = "Continuous Integration/Deployment" },
            new() { Name = "Docker", Code = "DOCKER", Category = SkillCategory.Technical, Description = "Container technologies" },
            new() { Name = "Azure", Code = "AZURE", Category = SkillCategory.Technical, Description = "Microsoft Azure cloud" },
        };

        _context.Skills.AddRange(skills);
        await _context.SaveChangesAsync();

        return skills.Count;
    }

    private async Task<int> SeedJobRolesAsync()
    {
        var existingRoles = await _context.JobRoles.CountAsync();
        if (existingRoles > 0)
        {
            _logger.LogInformation("Job roles already exist, skipping...");
            return 0;
        }

        // Get all skills
        var skills = await _context.Skills.ToListAsync();
        var skillDict = skills.ToDictionary(s => s.Code);

        var roles = new List<JobRole>
        {
            new()
            {
                Name = "Junior Developer",
                Code = "JR-DEV",
                Description = "Entry-level software developer",
                LevelInHierarchy = 1,
                SkillRequirements = new List<RoleSkillRequirement>
                {
                    new() { SkillId = skillDict["CSHARP"].Id, MinimumLevel = ProficiencyLevel.Assist, IsMandatory = true },
                    new() { SkillId = skillDict["ASPNET"].Id, MinimumLevel = ProficiencyLevel.Assist, IsMandatory = true },
                    new() { SkillId = skillDict["SQL"].Id, MinimumLevel = ProficiencyLevel.Follow, IsMandatory = true },
                    new() { SkillId = skillDict["GIT"].Id, MinimumLevel = ProficiencyLevel.Follow, IsMandatory = true },
                    new() { SkillId = skillDict["TEST"].Id, MinimumLevel = ProficiencyLevel.Follow, IsMandatory = false },
                }
            },
            new()
            {
                Name = "Mid-level Developer",
                Code = "MID-DEV",
                Description = "Experienced software developer",
                LevelInHierarchy = 2,
                SkillRequirements = new List<RoleSkillRequirement>
                {
                    new() { SkillId = skillDict["CSHARP"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["ASPNET"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["EF"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["SQL"].Id, MinimumLevel = ProficiencyLevel.Assist, IsMandatory = true },
                    new() { SkillId = skillDict["REACT"].Id, MinimumLevel = ProficiencyLevel.Assist, IsMandatory = true },
                    new() { SkillId = skillDict["API"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["GIT"].Id, MinimumLevel = ProficiencyLevel.Assist, IsMandatory = true },
                    new() { SkillId = skillDict["TEST"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["REVIEW"].Id, MinimumLevel = ProficiencyLevel.Assist, IsMandatory = false },
                }
            },
            new()
            {
                Name = "Senior Developer",
                Code = "SR-DEV",
                Description = "Senior software developer",
                LevelInHierarchy = 3,
                SkillRequirements = new List<RoleSkillRequirement>
                {
                    new() { SkillId = skillDict["CSHARP"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["ASPNET"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["EF"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["SQL"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["REACT"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["TS"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["API"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["GIT"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["TEST"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["REVIEW"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["CICD"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                    new() { SkillId = skillDict["AGILE"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = false },
                }
            },
            new()
            {
                Name = "Tech Lead",
                Code = "TECH-LEAD",
                Description = "Technical team leader",
                LevelInHierarchy = 4,
                SkillRequirements = new List<RoleSkillRequirement>
                {
                    new() { SkillId = skillDict["CSHARP"].Id, MinimumLevel = ProficiencyLevel.EnsureAdvise, IsMandatory = true },
                    new() { SkillId = skillDict["ASPNET"].Id, MinimumLevel = ProficiencyLevel.EnsureAdvise, IsMandatory = true },
                    new() { SkillId = skillDict["API"].Id, MinimumLevel = ProficiencyLevel.EnsureAdvise, IsMandatory = true },
                    new() { SkillId = skillDict["REVIEW"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["LEAD"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["AGILE"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["CICD"].Id, MinimumLevel = ProficiencyLevel.Enable, IsMandatory = true },
                    new() { SkillId = skillDict["AZURE"].Id, MinimumLevel = ProficiencyLevel.Apply, IsMandatory = true },
                }
            }
        };

        _context.JobRoles.AddRange(roles);
        await _context.SaveChangesAsync();

        return roles.Count;
    }

    private async Task<int> SeedTeamsAsync()
    {
        var existingTeams = await _context.Teams.CountAsync();
        if (existingTeams > 0)
        {
            _logger.LogInformation("Teams already exist, skipping...");
            return 0;
        }

        var teams = new List<Team>
        {
            new() { Name = "Platform Team", Description = "Core platform development" },
            new() { Name = "Product Team", Description = "Product features development" },
            new() { Name = "DevOps Team", Description = "Infrastructure and deployment" },
        };

        _context.Teams.AddRange(teams);
        await _context.SaveChangesAsync();

        return teams.Count;
    }

    private async Task<int> SeedEmployeesAsync()
    {
        var existingEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted);
        if (existingEmployees > 0)
        {
            _logger.LogInformation("Employees already exist, skipping...");
            return 0;
        }

        var roles = await _context.JobRoles.ToListAsync();
        var teams = await _context.Teams.ToListAsync();
        var skills = await _context.Skills.ToListAsync();

        var juniorRole = roles.First(r => r.Code == "JR-DEV");
        var midRole = roles.First(r => r.Code == "MID-DEV");
        var seniorRole = roles.First(r => r.Code == "SR-DEV");
        var techLeadRole = roles.First(r => r.Code == "TECH-LEAD");

        var employees = new List<Employee>
        {
            // Junior Developers
            new()
            {
                FullName = "Nguyen Van A",
                Email = "nguyenvana@example.com",
                JobRoleId = juniorRole.Id,
                TeamId = teams[0].Id,
                YearsOfExperience = 1,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.Follow,
                    ["ASPNET"] = ProficiencyLevel.Follow,
                    ["SQL"] = ProficiencyLevel.Follow,
                    ["GIT"] = ProficiencyLevel.Assist,
                })
            },
            new()
            {
                FullName = "Tran Thi B",
                Email = "tranthib@example.com",
                JobRoleId = juniorRole.Id,
                TeamId = teams[1].Id,
                YearsOfExperience = 1,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.Assist,
                    ["ASPNET"] = ProficiencyLevel.Assist,
                    ["SQL"] = ProficiencyLevel.Follow,
                    ["GIT"] = ProficiencyLevel.Follow,
                    ["REACT"] = ProficiencyLevel.Follow,
                })
            },
            // Mid-level Developers
            new()
            {
                FullName = "Le Van C",
                Email = "levanc@example.com",
                JobRoleId = midRole.Id,
                TeamId = teams[0].Id,
                YearsOfExperience = 3,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.Apply,
                    ["ASPNET"] = ProficiencyLevel.Apply,
                    ["EF"] = ProficiencyLevel.Assist,
                    ["SQL"] = ProficiencyLevel.Apply,
                    ["REACT"] = ProficiencyLevel.Assist,
                    ["API"] = ProficiencyLevel.Assist,
                    ["GIT"] = ProficiencyLevel.Apply,
                    ["TEST"] = ProficiencyLevel.Assist,
                })
            },
            new()
            {
                FullName = "Pham Thi D",
                Email = "phamthid@example.com",
                JobRoleId = midRole.Id,
                TeamId = teams[1].Id,
                YearsOfExperience = 4,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.Enable,
                    ["ASPNET"] = ProficiencyLevel.Apply,
                    ["EF"] = ProficiencyLevel.Apply,
                    ["SQL"] = ProficiencyLevel.Apply,
                    ["REACT"] = ProficiencyLevel.Apply,
                    ["TS"] = ProficiencyLevel.Assist,
                    ["API"] = ProficiencyLevel.Apply,
                    ["GIT"] = ProficiencyLevel.Apply,
                    ["TEST"] = ProficiencyLevel.Apply,
                })
            },
            // Senior Developers
            new()
            {
                FullName = "Hoang Van E",
                Email = "hoangvane@example.com",
                JobRoleId = seniorRole.Id,
                TeamId = teams[0].Id,
                YearsOfExperience = 6,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.Enable,
                    ["ASPNET"] = ProficiencyLevel.Enable,
                    ["EF"] = ProficiencyLevel.Enable,
                    ["SQL"] = ProficiencyLevel.Enable,
                    ["REACT"] = ProficiencyLevel.Apply,
                    ["TS"] = ProficiencyLevel.Apply,
                    ["API"] = ProficiencyLevel.Enable,
                    ["GIT"] = ProficiencyLevel.Enable,
                    ["TEST"] = ProficiencyLevel.Enable,
                    ["REVIEW"] = ProficiencyLevel.Apply,
                    ["CICD"] = ProficiencyLevel.Assist,
                })
            },
            new()
            {
                FullName = "Vo Thi F",
                Email = "vothif@example.com",
                JobRoleId = seniorRole.Id,
                TeamId = teams[2].Id,
                YearsOfExperience = 7,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.EnsureAdvise,
                    ["ASPNET"] = ProficiencyLevel.Enable,
                    ["EF"] = ProficiencyLevel.Enable,
                    ["SQL"] = ProficiencyLevel.Enable,
                    ["REACT"] = ProficiencyLevel.Enable,
                    ["TS"] = ProficiencyLevel.Enable,
                    ["API"] = ProficiencyLevel.Enable,
                    ["GIT"] = ProficiencyLevel.Enable,
                    ["TEST"] = ProficiencyLevel.Enable,
                    ["REVIEW"] = ProficiencyLevel.Enable,
                    ["CICD"] = ProficiencyLevel.Enable,
                    ["DOCKER"] = ProficiencyLevel.Apply,
                })
            },
            // Tech Lead
            new()
            {
                FullName = "Dao Van G",
                Email = "daovang@example.com",
                JobRoleId = techLeadRole.Id,
                TeamId = teams[0].Id,
                YearsOfExperience = 10,
                Skills = CreateEmployeeSkills(skills, new Dictionary<string, ProficiencyLevel>
                {
                    ["CSHARP"] = ProficiencyLevel.EnsureAdvise,
                    ["ASPNET"] = ProficiencyLevel.EnsureAdvise,
                    ["API"] = ProficiencyLevel.EnsureAdvise,
                    ["REVIEW"] = ProficiencyLevel.Enable,
                    ["LEAD"] = ProficiencyLevel.Apply,
                    ["AGILE"] = ProficiencyLevel.Enable,
                    ["CICD"] = ProficiencyLevel.Enable,
                    ["AZURE"] = ProficiencyLevel.Apply,
                    ["DOCKER"] = ProficiencyLevel.Enable,
                })
            },
        };

        _context.Employees.AddRange(employees);
        await _context.SaveChangesAsync();

        return employees.Count;
    }

    private List<EmployeeSkill> CreateEmployeeSkills(List<Skill> allSkills, Dictionary<string, ProficiencyLevel> skillLevels)
    {
        var employeeSkills = new List<EmployeeSkill>();

        foreach (var (code, level) in skillLevels)
        {
            var skill = allSkills.FirstOrDefault(s => s.Code == code);
            if (skill != null)
            {
                employeeSkills.Add(new EmployeeSkill
                {
                    SkillId = skill.Id,
                    CurrentLevel = level,
                    SelfAssessedLevel = level,
                    IsValidated = false
                });
            }
        }

        return employeeSkills;
    }
}

public class SeedResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SkillsCreated { get; set; }
    public int JobRolesCreated { get; set; }
    public int TeamsCreated { get; set; }
    public int EmployeesCreated { get; set; }
    public int GapsCreated { get; set; }
    public int GapsUpdated { get; set; }
}
