using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Learning;

/// <summary>
/// Identified skill gap for an employee
/// Compares current level vs required/expected level for their role
/// </summary>
public class SkillGap : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid SkillId { get; set; }
    public Guid? JobRoleId { get; set; }  // Role that defines the requirement

    // Gap details
    public ProficiencyLevel CurrentLevel { get; set; }
    public ProficiencyLevel RequiredLevel { get; set; }
    public int GapSize { get; set; }  // RequiredLevel - CurrentLevel
    public GapPriority Priority { get; set; }
    public bool IsAddressed { get; set; }  // Has learning path been created

    // AI Analysis
    public string? AiAnalysis { get; set; }       // Why this gap matters
    public string? AiRecommendation { get; set; } // What to do about it

    // Timestamps
    public DateTime IdentifiedAt { get; set; }
    public DateTime? AddressedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }  // When gap is closed

    // Navigation
    public Organization.Employee Employee { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
    public Organization.JobRole? JobRole { get; set; }
    public ICollection<EmployeeLearningPath> LearningPaths { get; set; } = new List<EmployeeLearningPath>();
}

/// <summary>
/// Team-level skill gap analysis
/// Aggregate view of gaps across a team
/// </summary>
public class TeamSkillGap : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid SkillId { get; set; }

    // Gap metrics
    public int TotalMembers { get; set; }
    public int MembersWithGap { get; set; }
    public double AverageGapSize { get; set; }
    public GapPriority OverallPriority { get; set; }

    // Analysis
    public string? AiAnalysis { get; set; }
    public DateTime AnalyzedAt { get; set; }

    // Navigation
    public Organization.Team Team { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}
