using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Organization;

/// <summary>
/// Project entity for tracking skill application and requirements
/// Integrates with daily task app
/// </summary>
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExternalId { get; set; }  // ID from daily task app
    public Guid? TeamId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    // Navigation
    public Team? Team { get; set; }
    public ICollection<ProjectSkillRequirement> SkillRequirements { get; set; } = new List<ProjectSkillRequirement>();
    public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
}

/// <summary>
/// Skills required for a project
/// Used for matching employees to projects based on skills
/// </summary>
public class ProjectSkillRequirement : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel MinimumLevel { get; set; }
    public int RequiredHeadcount { get; set; } = 1;
    public bool IsCritical { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}

/// <summary>
/// Employee assignment to project
/// For tracking skill utilization
/// </summary>
public class ProjectAssignment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; }  // Role in project
    public DateTime AssignedDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int AllocationPercentage { get; set; } = 100;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Project Project { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}
