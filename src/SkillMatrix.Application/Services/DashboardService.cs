using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Dashboard;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly SkillMatrixDbContext _context;

    public DashboardService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync()
    {
        var overview = new DashboardOverviewDto();

        // Tổng số nhân viên active
        overview.TotalEmployees = await _context.Employees
            .CountAsync(e => !e.IsDeleted && e.Status == EmploymentStatus.Active);

        // Tổng số skills
        overview.TotalSkills = await _context.Skills
            .CountAsync(s => !s.IsDeleted && s.IsActive);

        // Tổng số assessments hoàn thành
        overview.TotalAssessments = await _context.Assessments
            .CountAsync(a => !a.IsDeleted && a.Status == AssessmentStatus.Completed);

        // Tổng số test templates
        overview.TotalTestTemplates = await _context.TestTemplates
            .CountAsync(t => !t.IsDeleted && t.IsActive);

        // Phân bố nhân sự theo team
        var teamGroups = await _context.Employees
            .Where(e => !e.IsDeleted && e.Status == EmploymentStatus.Active)
            .GroupBy(e => new { e.TeamId, TeamName = e.Team != null ? e.Team.Name : "Chưa phân team" })
            .Select(g => new TeamDistributionDto
            {
                TeamId = g.Key.TeamId,
                TeamName = g.Key.TeamName,
                EmployeeCount = g.Count()
            })
            .ToListAsync();

        var totalEmployeesForTeam = teamGroups.Sum(t => t.EmployeeCount);
        foreach (var team in teamGroups)
        {
            team.Percentage = totalEmployeesForTeam > 0
                ? Math.Round((double)team.EmployeeCount / totalEmployeesForTeam * 100, 1)
                : 0;
        }
        overview.TeamDistribution = teamGroups;

        // Phân bố nhân sự theo role
        var roleGroups = await _context.Employees
            .Where(e => !e.IsDeleted && e.Status == EmploymentStatus.Active)
            .GroupBy(e => e.SystemRole)
            .Select(g => new RoleDistributionDto
            {
                RoleName = g.Key.ToString(),
                EmployeeCount = g.Count()
            })
            .ToListAsync();

        var totalEmployeesForRole = roleGroups.Sum(r => r.EmployeeCount);
        foreach (var role in roleGroups)
        {
            role.Percentage = totalEmployeesForRole > 0
                ? Math.Round((double)role.EmployeeCount / totalEmployeesForRole * 100, 1)
                : 0;
        }
        overview.RoleDistribution = roleGroups;

        // Top skills phổ biến (top 10)
        overview.TopSkills = await _context.EmployeeSkills
            .Where(es => !es.IsDeleted && !es.Employee.IsDeleted && es.Employee.Status == EmploymentStatus.Active)
            .GroupBy(es => new
            {
                es.SkillId,
                es.Skill.Name,
                es.Skill.Code,
                DomainName = es.Skill.Subcategory.SkillDomain.Name
            })
            .Select(g => new SkillPopularityDto
            {
                SkillId = g.Key.SkillId,
                SkillName = g.Key.Name,
                SkillCode = g.Key.Code,
                DomainName = g.Key.DomainName,
                EmployeeCount = g.Count(),
                AverageLevel = g.Average(es => (int)es.CurrentLevel)
            })
            .OrderByDescending(s => s.EmployeeCount)
            .Take(10)
            .ToListAsync();

        // Phân bố proficiency levels
        var levelGroups = await _context.EmployeeSkills
            .Where(es => !es.IsDeleted && !es.Employee.IsDeleted && es.Employee.Status == EmploymentStatus.Active)
            .GroupBy(es => es.CurrentLevel)
            .Select(g => new ProficiencyDistributionDto
            {
                Level = (int)g.Key,
                LevelName = g.Key.ToString(),
                EmployeeSkillCount = g.Count()
            })
            .ToListAsync();

        var totalSkillRecords = levelGroups.Sum(l => l.EmployeeSkillCount);
        foreach (var level in levelGroups)
        {
            level.Percentage = totalSkillRecords > 0
                ? Math.Round((double)level.EmployeeSkillCount / totalSkillRecords * 100, 1)
                : 0;
        }
        overview.ProficiencyDistribution = levelGroups.OrderBy(l => l.Level).ToList();

        // Recent assessments (10 gần nhất)
        overview.RecentAssessments = await _context.Assessments
            .Where(a => !a.IsDeleted && a.Status == AssessmentStatus.Completed && a.CompletedAt != null)
            .OrderByDescending(a => a.CompletedAt)
            .Take(10)
            .Select(a => new RecentAssessmentDto
            {
                Id = a.Id,
                EmployeeName = a.Employee.FullName,
                TestTitle = a.TestTemplate != null ? a.TestTemplate.Title : a.Title ?? "Assessment",
                Score = a.Score,
                MaxScore = a.MaxScore,
                Percentage = a.MaxScore > 0 ? Math.Round((double)(a.Score ?? 0) / a.MaxScore.Value * 100, 1) : 0,
                Passed = a.TestTemplate != null && a.Score != null && a.MaxScore != null
                    ? (double)a.Score.Value / a.MaxScore.Value * 100 >= a.TestTemplate.PassingScore
                    : false,
                CompletedAt = a.CompletedAt ?? DateTime.UtcNow
            })
            .ToListAsync();

        // Domain skill coverage
        overview.DomainSkillCoverage = await _context.SkillDomains
            .Where(d => !d.IsDeleted && d.IsActive)
            .Select(d => new DomainSkillCoverageDto
            {
                DomainId = d.Id,
                DomainName = d.Name,
                DomainCode = d.Code,
                TotalSkills = d.Subcategories
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .SelectMany(s => s.Skills)
                    .Count(sk => !sk.IsDeleted && sk.IsActive),
                EmployeesWithSkills = _context.EmployeeSkills
                    .Count(es => !es.IsDeleted
                        && es.Skill.Subcategory.SkillDomainId == d.Id
                        && !es.Employee.IsDeleted
                        && es.Employee.Status == EmploymentStatus.Active)
            })
            .ToListAsync();

        foreach (var domain in overview.DomainSkillCoverage)
        {
            var maxPossible = domain.TotalSkills * overview.TotalEmployees;
            domain.CoveragePercentage = maxPossible > 0
                ? Math.Round((double)domain.EmployeesWithSkills / maxPossible * 100, 1)
                : 0;
        }

        return overview;
    }

    public async Task<List<EmployeeSkillSummaryDto>> GetEmployeeSkillsAsync(Guid? teamId = null)
    {
        var query = _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Include(e => e.Skills)
                .ThenInclude(s => s.Skill)
                    .ThenInclude(s => s.Subcategory)
                        .ThenInclude(sc => sc.SkillDomain)
            .Where(e => !e.IsDeleted && e.Status == EmploymentStatus.Active);

        if (teamId.HasValue)
        {
            query = query.Where(e => e.TeamId == teamId);
        }

        var employees = await query.ToListAsync();

        return employees.Select(e => new EmployeeSkillSummaryDto
        {
            EmployeeId = e.Id,
            EmployeeName = e.FullName,
            TeamName = e.Team?.Name,
            JobRoleName = e.JobRole?.Name,
            TotalSkills = e.Skills.Count(s => !s.IsDeleted),
            AverageLevel = e.Skills.Any(s => !s.IsDeleted)
                ? Math.Round(e.Skills.Where(s => !s.IsDeleted).Average(s => (int)s.CurrentLevel), 1)
                : 0,
            Skills = e.Skills
                .Where(s => !s.IsDeleted)
                .Select(s => new EmployeeSkillItemDto
                {
                    SkillId = s.SkillId,
                    SkillName = s.Skill.Name,
                    SkillCode = s.Skill.Code,
                    DomainName = s.Skill.Subcategory.SkillDomain.Name,
                    CurrentLevel = (int)s.CurrentLevel,
                    LevelName = s.CurrentLevel.ToString(),
                    IsValidated = s.IsValidated,
                    LastAssessedAt = s.LastAssessedAt
                })
                .OrderBy(s => s.DomainName)
                .ThenBy(s => s.SkillName)
                .ToList()
        })
        .OrderBy(e => e.EmployeeName)
        .ToList();
    }

    public async Task<EmployeeSkillSummaryDto?> GetEmployeeSkillDetailAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Include(e => e.Skills)
                .ThenInclude(s => s.Skill)
                    .ThenInclude(s => s.Subcategory)
                        .ThenInclude(sc => sc.SkillDomain)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null) return null;

        return new EmployeeSkillSummaryDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            TeamName = employee.Team?.Name,
            JobRoleName = employee.JobRole?.Name,
            TotalSkills = employee.Skills.Count(s => !s.IsDeleted),
            AverageLevel = employee.Skills.Any(s => !s.IsDeleted)
                ? Math.Round(employee.Skills.Where(s => !s.IsDeleted).Average(s => (int)s.CurrentLevel), 1)
                : 0,
            Skills = employee.Skills
                .Where(s => !s.IsDeleted)
                .Select(s => new EmployeeSkillItemDto
                {
                    SkillId = s.SkillId,
                    SkillName = s.Skill.Name,
                    SkillCode = s.Skill.Code,
                    DomainName = s.Skill.Subcategory.SkillDomain.Name,
                    CurrentLevel = (int)s.CurrentLevel,
                    LevelName = s.CurrentLevel.ToString(),
                    IsValidated = s.IsValidated,
                    LastAssessedAt = s.LastAssessedAt
                })
                .OrderBy(s => s.DomainName)
                .ThenBy(s => s.SkillName)
                .ToList()
        };
    }

    public async Task<TeamSkillMatrixDto> GetTeamSkillMatrixAsync(Guid? teamId = null)
    {
        // Get employees
        var employeesQuery = _context.Employees
            .Include(e => e.Skills)
            .Where(e => !e.IsDeleted && e.Status == EmploymentStatus.Active);

        if (teamId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.TeamId == teamId);
        }

        var employees = await employeesQuery.ToListAsync();

        // Get all skill IDs that any employee has
        var skillIds = employees
            .SelectMany(e => e.Skills.Where(s => !s.IsDeleted).Select(s => s.SkillId))
            .Distinct()
            .ToList();

        // Get skills info
        var skills = await _context.Skills
            .Where(s => skillIds.Contains(s.Id) && !s.IsDeleted)
            .Select(s => new SkillColumnDto
            {
                SkillId = s.Id,
                SkillName = s.Name,
                SkillCode = s.Code
            })
            .OrderBy(s => s.SkillName)
            .ToListAsync();

        // Get team name
        var teamName = "Tất cả";
        if (teamId.HasValue)
        {
            var team = await _context.Teams.FindAsync(teamId.Value);
            teamName = team?.Name ?? "Unknown";
        }

        return new TeamSkillMatrixDto
        {
            TeamId = teamId,
            TeamName = teamName,
            Skills = skills,
            Employees = employees.Select(e => new EmployeeSkillRowDto
            {
                EmployeeId = e.Id,
                EmployeeName = e.FullName,
                SkillLevels = e.Skills
                    .Where(s => !s.IsDeleted)
                    .ToDictionary(s => s.SkillId.ToString(), s => (int)s.CurrentLevel)
            })
            .OrderBy(e => e.EmployeeName)
            .ToList()
        };
    }
}
