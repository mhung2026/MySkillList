using SkillMatrix.Domain.Common;

namespace SkillMatrix.Domain.Entities.Taxonomy;

/// <summary>
/// Top-level grouping of skills (SFIA: Categories)
/// Examples: "Development & Implementation", "Strategy & Architecture", "People & Skills"
/// Extensible for company-specific domains
/// </summary>
public class SkillDomain : VersionedEntity
{
    public string Code { get; set; } = string.Empty;  // e.g., "DEV", "STRA", "PEOP"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SkillSubcategory> Subcategories { get; set; } = new List<SkillSubcategory>();
}

/// <summary>
/// Second-level grouping (SFIA: Subcategories)
/// Examples under "Development & Implementation": "Systems Development", "User Experience", "Installation & Integration"
/// </summary>
public class SkillSubcategory : VersionedEntity
{
    public Guid SkillDomainId { get; set; }
    public string Code { get; set; } = string.Empty;  // e.g., "SYSDEV", "UX", "INST"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public SkillDomain SkillDomain { get; set; } = null!;
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
