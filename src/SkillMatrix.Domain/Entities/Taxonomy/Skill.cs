using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Entities.Organization;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Taxonomy;

/// <summary>
/// Individual skill definition
/// Examples: "Programming/Software Development", "Software Design", "Testing"
/// </summary>
public class Skill : VersionedEntity
{
    public Guid SubcategoryId { get; set; }

    // Identification
    public string Code { get; set; } = string.Empty;       // e.g., "PROG", "SWDN", "TEST"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Classification
    public SkillCategory Category { get; set; }
    public SkillType SkillType { get; set; }  // Core, Specialty, Adjacent

    // Metadata
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCompanySpecific { get; set; }  // For internal tools, proprietary frameworks
    public string? Tags { get; set; }  // JSON array of tags for search/filter
    public string? ApplicableLevels { get; set; }  // Proficiency levels applicable for this skill, e.g., "2,3,4,5,6"

    // Navigation
    public SkillSubcategory Subcategory { get; set; } = null!;
    public ICollection<SkillLevelDefinition> LevelDefinitions { get; set; } = new List<SkillLevelDefinition>();
    public ICollection<RoleSkillRequirement> RoleRequirements { get; set; } = new List<RoleSkillRequirement>();
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
    public ICollection<SkillRelationship> RelatedSkillsFrom { get; set; } = new List<SkillRelationship>();
    public ICollection<SkillRelationship> RelatedSkillsTo { get; set; } = new List<SkillRelationship>();
}

/// <summary>
/// Behavioral anchors for each proficiency level of a skill
/// This is what makes assessment objective - clear criteria for each level
/// </summary>
public class SkillLevelDefinition : BaseEntity
{
    public Guid SkillId { get; set; }
    public ProficiencyLevel Level { get; set; }

    // Custom level name (overrides default level name)
    public string? CustomLevelName { get; set; }  // e.g., "Junior Developer" instead of "Apply"

    // Level descriptions
    public string Description { get; set; } = string.Empty;  // General description of this level
    public string? Autonomy { get; set; }          // Level of independence
    public string? Influence { get; set; }         // Scope of impact
    public string? Complexity { get; set; }        // Task complexity handled
    public string? BusinessSkills { get; set; }    // Related business skills expected
    public string? Knowledge { get; set; }         // Required knowledge

    // For assessment/validation
    public string? BehavioralIndicators { get; set; }  // JSON array of observable behaviors
    public string? EvidenceExamples { get; set; }      // JSON array of evidence examples

    // Navigation
    public Skill Skill { get; set; } = null!;
}

/// <summary>
/// Relationships between skills (prerequisites, related, etc.)
/// </summary>
public class SkillRelationship : BaseEntity
{
    public Guid FromSkillId { get; set; }
    public Guid ToSkillId { get; set; }
    public SkillRelationshipType RelationshipType { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Skill FromSkill { get; set; } = null!;
    public Skill ToSkill { get; set; } = null!;
}

public enum SkillRelationshipType
{
    Prerequisite = 1,   // Must have skill A before B
    Related = 2,        // Skills often go together
    Alternative = 3,    // One or the other
    Enhances = 4        // Having A makes B more effective
}

/// <summary>
/// Proficiency Level Definition - Defines available proficiency levels
/// Can be extended beyond standard 7 levels (supports SFIA, custom frameworks, etc.)
/// </summary>
public class ProficiencyLevelDefinition : BaseEntity
{
    public int Level { get; set; }  // Level number (1, 2, 3... unlimited)
    public string LevelName { get; set; } = string.Empty;  // e.g., "Follow", "Assist", "Apply"
    public string? Description { get; set; }
    public string? Autonomy { get; set; }
    public string? Influence { get; set; }
    public string? Complexity { get; set; }
    public string? Knowledge { get; set; }
    public string? BusinessSkills { get; set; }
    public string? BehavioralIndicators { get; set; }  // JSON array
    public string? Color { get; set; }  // Display color for UI
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
