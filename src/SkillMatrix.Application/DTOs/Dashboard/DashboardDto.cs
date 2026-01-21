namespace SkillMatrix.Application.DTOs.Dashboard;

/// <summary>
/// Tổng quan Dashboard
/// </summary>
public class DashboardOverviewDto
{
    // Thống kê tổng quan
    public int TotalEmployees { get; set; }
    public int TotalSkills { get; set; }
    public int TotalAssessments { get; set; }
    public int TotalTestTemplates { get; set; }

    // Phân bố nhân sự theo team
    public List<TeamDistributionDto> TeamDistribution { get; set; } = new();

    // Phân bố nhân sự theo role
    public List<RoleDistributionDto> RoleDistribution { get; set; } = new();

    // Top skills phổ biến
    public List<SkillPopularityDto> TopSkills { get; set; } = new();

    // Skill gaps (skills thiếu nhiều nhất)
    public List<SkillGapDto> SkillGaps { get; set; } = new();

    // Phân bố proficiency levels
    public List<ProficiencyDistributionDto> ProficiencyDistribution { get; set; } = new();

    // Recent assessments
    public List<RecentAssessmentDto> RecentAssessments { get; set; } = new();

    // Employees by skill domain
    public List<DomainSkillCoverageDto> DomainSkillCoverage { get; set; } = new();
}

/// <summary>
/// Phân bố nhân sự theo team
/// </summary>
public class TeamDistributionDto
{
    public Guid? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Phân bố nhân sự theo role
/// </summary>
public class RoleDistributionDto
{
    public string RoleName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Skill phổ biến nhất
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
/// Skill Gap - skills thiếu nhiều nhất
/// </summary>
public class SkillGapDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public int RequiredCount { get; set; }  // Số người cần skill này
    public int HasSkillCount { get; set; }  // Số người có skill này
    public int GapCount { get; set; }       // Số người thiếu
    public double GapPercentage { get; set; }
}

/// <summary>
/// Phân bố proficiency levels
/// </summary>
public class ProficiencyDistributionDto
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int EmployeeSkillCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Assessment gần đây
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
/// Độ phủ skill theo domain
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
/// Chi tiết skill của một nhân viên
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
/// Chi tiết từng skill của nhân viên
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
/// Skill matrix của team
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
