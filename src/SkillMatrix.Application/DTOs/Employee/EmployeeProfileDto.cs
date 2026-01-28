using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Employee;

#region Skill Profile DTOs

/// <summary>
/// Comprehensive skill profile for an employee
/// </summary>
public class SkillProfileDto
{
    public EmployeeBasicDto Employee { get; set; } = null!;
    public List<EmployeeSkillDetailDto> Skills { get; set; } = new();
    public SkillProfileSummaryDto Summary { get; set; } = null!;
    public List<ProfileAssessmentDto> RecentAssessments { get; set; } = new();
}

/// <summary>
/// Basic employee info
/// </summary>
public class EmployeeBasicDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RoleBasicDto? JobRole { get; set; }
    public TeamBasicDto? Team { get; set; }
    public int? YearsOfExperience { get; set; }
}

/// <summary>
/// Basic role info
/// </summary>
public class RoleBasicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? LevelInHierarchy { get; set; }
}

/// <summary>
/// Basic team info
/// </summary>
public class TeamBasicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Detailed skill info with all assessment levels
/// </summary>
public class EmployeeSkillDetailDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? DomainName { get; set; }
    public int CurrentLevel { get; set; }
    public string CurrentLevelName { get; set; } = string.Empty;
    public int? SelfAssessedLevel { get; set; }
    public int? ManagerAssessedLevel { get; set; }
    public int? TestValidatedLevel { get; set; }
    public bool IsValidated { get; set; }
    public DateTime? LastAssessedAt { get; set; }
}

/// <summary>
/// Summary of employee's skill profile
/// </summary>
public class SkillProfileSummaryDto
{
    public int TotalSkills { get; set; }
    public int ValidatedSkills { get; set; }
    public double AverageLevel { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

/// <summary>
/// Recent assessment summary
/// </summary>
public class ProfileAssessmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    public double? Percentage { get; set; }
    public string Status { get; set; } = string.Empty;
}

#endregion

#region Gap Analysis DTOs

/// <summary>
/// Gap analysis result for an employee
/// </summary>
public class GapAnalysisDto
{
    public EmployeeBasicDto Employee { get; set; } = null!;
    public RoleBasicDto? CurrentRole { get; set; }
    public RoleBasicDto TargetRole { get; set; } = null!;
    public List<SkillGapDetailDto> Gaps { get; set; } = new();
    public GapAnalysisSummaryDto Summary { get; set; } = null!;
}

/// <summary>
/// Detailed skill gap info
/// </summary>
public class SkillGapDetailDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public string CurrentLevelName { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public string RequiredLevelName { get; set; } = string.Empty;
    public int? ExpectedLevel { get; set; }
    public int GapSize { get; set; }
    public string Priority { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsMet { get; set; } // True if current level meets or exceeds required level
    public string? AiAnalysis { get; set; }
    public string? AiRecommendation { get; set; }
}

/// <summary>
/// Summary of gap analysis
/// </summary>
public class GapAnalysisSummaryDto
{
    public int TotalGaps { get; set; }
    public int CriticalGaps { get; set; }
    public int HighGaps { get; set; }
    public int MediumGaps { get; set; }
    public int LowGaps { get; set; }
    public int MetRequirements { get; set; }
    public double OverallReadiness { get; set; }
}

/// <summary>
/// Request to recalculate gaps
/// </summary>
public class RecalculateGapsRequest
{
    public Guid? TargetRoleId { get; set; }
}

/// <summary>
/// Result of gap recalculation
/// </summary>
public class RecalculateGapsResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int GapsCreated { get; set; }
    public int GapsUpdated { get; set; }
    public int GapsResolved { get; set; }
}

/// <summary>
/// Result of bulk gap recalculation for all employees
/// </summary>
public class BulkRecalculateGapsResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int EmployeesProcessed { get; set; }
    public int EmployeesWithGaps { get; set; }
    public int TotalGapsCreated { get; set; }
    public int TotalGapsUpdated { get; set; }
    public int TotalGapsResolved { get; set; }
    public List<EmployeeBulkGapResult> EmployeeResults { get; set; } = new();
}

/// <summary>
/// Individual employee result in bulk operation
/// </summary>
public class EmployeeBulkGapResult
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? RoleName { get; set; }
    public int GapsCreated { get; set; }
    public int GapsUpdated { get; set; }
    public int GapsResolved { get; set; }
}

#endregion

#region Learning Path DTOs

/// <summary>
/// Request to create a learning path
/// </summary>
public class CreateLearningPathRequest
{
    public Guid? SkillGapId { get; set; }
    public Guid? TargetSkillId { get; set; }
    public int TargetLevel { get; set; }
    public DateTime? TargetCompletionDate { get; set; }
    public int? TimeConstraintMonths { get; set; }
    public bool? UseAiGeneration { get; set; } = true;
}

/// <summary>
/// Learning path response
/// </summary>
public class LearningPathDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillBasicDto? TargetSkill { get; set; }
    public int? CurrentLevel { get; set; }
    public string? CurrentLevelName { get; set; }
    public int TargetLevel { get; set; }
    public string TargetLevelName { get; set; } = string.Empty;
    public int? EstimatedTotalHours { get; set; }
    public int? EstimatedDurationWeeks { get; set; }
    public DateTime? TargetCompletionDate { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? AiRationale { get; set; }
    public List<string>? KeySuccessFactors { get; set; }
    public List<string>? PotentialChallenges { get; set; }
    public List<LearningPathItemDto> Items { get; set; } = new();
    public List<LearningPathMilestoneDto>? Milestones { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Basic skill info
/// </summary>
public class SkillBasicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Learning path item
/// </summary>
public class LearningPathItemDto
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public Guid? ResourceId { get; set; }
    public string? ExternalUrl { get; set; }
    public int? EstimatedHours { get; set; }
    public int? TargetLevelAfter { get; set; }
    public string? SuccessCriteria { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Learning path milestone
/// </summary>
public class LearningPathMilestoneDto
{
    public int AfterItem { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ExpectedLevel { get; set; }
}

/// <summary>
/// AI-generated learning recommendation
/// </summary>
public class LearningRecommendationDto
{
    public Guid Id { get; set; }
    public Guid SkillGapId { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;  // Course, Project, Mentorship
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }  // Coursera course URL, etc.
    public int? EstimatedHours { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime GeneratedAt { get; set; }
}

#endregion
