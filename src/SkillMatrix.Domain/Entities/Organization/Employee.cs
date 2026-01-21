using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Organization;

/// <summary>
/// Employee entity - central user profile
/// </summary>
public class Employee : BaseEntity
{
    // Identity (linked to Auth system)
    public string UserId { get; set; } = string.Empty;  // External auth ID if any
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  // For simple auth
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    // Organization
    public Guid? TeamId { get; set; }
    public Guid? JobRoleId { get; set; }
    public Guid? ManagerId { get; set; }
    public UserRole SystemRole { get; set; } = UserRole.Employee;
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    // Employment info
    public DateTime? JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public int YearsOfExperience { get; set; }

    // Navigation
    public Team? Team { get; set; }
    public JobRole? JobRole { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    // Skill-related
    public ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
    public ICollection<Assessment.Assessment> Assessments { get; set; } = new List<Assessment.Assessment>();
    public ICollection<Learning.EmployeeLearningPath> LearningPaths { get; set; } = new List<Learning.EmployeeLearningPath>();
    public ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
}

/// <summary>
/// Employee's current skill profile
/// Tracks the assessed/validated skill level for each employee
/// </summary>
public class EmployeeSkill : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid SkillId { get; set; }

    // Current assessed level
    public ProficiencyLevel CurrentLevel { get; set; }
    public ProficiencyLevel? SelfAssessedLevel { get; set; }
    public ProficiencyLevel? ManagerAssessedLevel { get; set; }
    public ProficiencyLevel? TestValidatedLevel { get; set; }

    // Evidence & validation
    public string? Evidence { get; set; }  // JSON: projects, certifications, etc.
    public DateTime? LastAssessedAt { get; set; }
    public Guid? LastAssessmentId { get; set; }
    public bool IsValidated { get; set; }  // Manager reviewed

    // History tracking
    public ProficiencyLevel? PreviousLevel { get; set; }
    public DateTime? LevelChangedAt { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}

/// <summary>
/// Historical record of skill level changes
/// For tracking growth over time
/// </summary>
public class EmployeeSkillHistory : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel FromLevel { get; set; }
    public ProficiencyLevel ToLevel { get; set; }
    public string ChangeReason { get; set; } = string.Empty;  // Assessment, Training, Project experience
    public Guid? AssessmentId { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}
