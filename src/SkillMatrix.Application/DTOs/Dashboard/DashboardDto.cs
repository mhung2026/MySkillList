namespace SkillMatrix.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard Overview
/// </summary>
public class DashboardOverviewDto
{
    // Overview statistics
    public int TotalEmployees { get; set; }
    public int TotalSkills { get; set; }
    public int TotalAssessments { get; set; }
    public int TotalTestTemplates { get; set; }

    // Employee distribution by team
    public List<TeamDistributionDto> TeamDistribution { get; set; } = new();

    // Employee distribution by role
    public List<RoleDistributionDto> RoleDistribution { get; set; } = new();

    // Top popular skills
    public List<SkillPopularityDto> TopSkills { get; set; } = new();

    // Skill gaps (most missing skills)
    public List<SkillGapDto> SkillGaps { get; set; } = new();

    // Proficiency levels distribution
    public List<ProficiencyDistributionDto> ProficiencyDistribution { get; set; } = new();

    // Recent assessments
    public List<RecentAssessmentDto> RecentAssessments { get; set; } = new();

    // Employees by skill domain
    public List<DomainSkillCoverageDto> DomainSkillCoverage { get; set; } = new();
}

/// <summary>
/// Employee distribution by team
/// </summary>
public class TeamDistributionDto
{
    public Guid? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Employee distribution by role
/// </summary>
public class RoleDistributionDto
{
    public string RoleName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Most popular skill
/// </summary>
public class SkillPopularityDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public double AverageLevel { get; set; }
}

/// <summary>
/// Skill Gap - most missing skills
/// </summary>
public class SkillGapDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public int RequiredCount { get; set; }  // Number of people who need this skill
    public int HasSkillCount { get; set; }  // Number of people who have this skill
    public int GapCount { get; set; }       // Number of people missing the skill
    public double GapPercentage { get; set; }
}

/// <summary>
/// Proficiency levels distribution
/// </summary>
public class ProficiencyDistributionDto
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int EmployeeSkillCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Recent assessment
/// </summary>
public class RecentAssessmentDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string TestTitle { get; set; } = string.Empty;
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    public double? Percentage { get; set; }
    public bool Passed { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Skill coverage by domain
/// </summary>
public class DomainSkillCoverageDto
{
    public Guid DomainId { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public int TotalSkills { get; set; }
    public int EmployeesWithSkills { get; set; }
    public double CoveragePercentage { get; set; }
}

/// <summary>
/// Skill details for an employee
/// </summary>
public class EmployeeSkillSummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? TeamName { get; set; }
    public string? JobRoleName { get; set; }
    public int TotalSkills { get; set; }
    public double AverageLevel { get; set; }
    public List<EmployeeSkillItemDto> Skills { get; set; } = new();
}

/// <summary>
/// Individual skill details for an employee
/// </summary>
public class EmployeeSkillItemDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public bool IsValidated { get; set; }
    public DateTime? LastAssessedAt { get; set; }
}

/// <summary>
/// Team skill matrix
/// </summary>
public class TeamSkillMatrixDto
{
    public Guid? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public List<SkillColumnDto> Skills { get; set; } = new();
    public List<EmployeeSkillRowDto> Employees { get; set; } = new();
}

public class SkillColumnDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
}

public class EmployeeSkillRowDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Dictionary<string, int> SkillLevels { get; set; } = new();  // SkillId -> Level
}
