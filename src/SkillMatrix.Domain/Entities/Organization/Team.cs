using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Organization;

/// <summary>
/// Team/Department entity
/// Examples: "Team SoEzy", "Team Mezy"
/// </summary>
public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentTeamId { get; set; }  // For hierarchical structure
    public Guid? TeamLeadId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Team? ParentTeam { get; set; }
    public ICollection<Team> SubTeams { get; set; } = new List<Team>();
    public Employee? TeamLead { get; set; }
    public ICollection<Employee> Members { get; set; } = new List<Employee>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

/// <summary>
/// Job Role/Position definition
/// Examples: "Backend Developer", "Frontend Developer", "QA Engineer", "BA", "PM"
/// </summary>
public class JobRole : VersionedEntity
{
    public string Code { get; set; } = string.Empty;  // e.g., "BE", "FE", "QA", "BA", "PM"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentRoleId { get; set; }  // For role hierarchy (e.g., Senior BE -> BE)
    public int LevelInHierarchy { get; set; }  // Junior=1, Mid=2, Senior=3, Lead=4
    public bool IsActive { get; set; } = true;

    // Navigation
    public JobRole? ParentRole { get; set; }
    public ICollection<JobRole> ChildRoles { get; set; } = new List<JobRole>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<RoleSkillRequirement> SkillRequirements { get; set; } = new List<RoleSkillRequirement>();
}

/// <summary>
/// Skill requirements for a job role (Career Ladder mapping)
/// Defines what skills and at what level are required/expected for each role
/// </summary>
public class RoleSkillRequirement : BaseEntity
{
    public Guid JobRoleId { get; set; }
    public Guid SkillId { get; set; }

    public ProficiencyLevel MinimumLevel { get; set; }     // Must have
    public ProficiencyLevel? ExpectedLevel { get; set; }   // Should have
    public ProficiencyLevel? ExpertLevel { get; set; }     // Nice to have/expert
    public bool IsMandatory { get; set; }
    public int Priority { get; set; }  // For ranking importance

    // Navigation
    public JobRole JobRole { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}
