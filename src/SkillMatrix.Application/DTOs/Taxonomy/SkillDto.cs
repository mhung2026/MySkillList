using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Taxonomy;

public class SkillDto
{
    public Guid Id { get; set; }
    public Guid SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public SkillType SkillType { get; set; }
    public string SkillTypeName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompanySpecific { get; set; }
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Proficiency levels applicable for this skill (e.g., "2,3,4,5,6")
    /// </summary>
    public string? ApplicableLevelsString { get; set; }

    /// <summary>
    /// Parsed list of applicable levels
    /// </summary>
    public List<int>? ApplicableLevels { get; set; }

    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Level definitions - detailed criteria for each applicable level
    public List<SkillLevelDefinitionDto> LevelDefinitions { get; set; } = new();
}

public class SkillListDto
{
    public Guid Id { get; set; }
    public Guid SubcategoryId { get; set; }
    public string SubcategoryCode { get; set; } = string.Empty;
    public string SubcategoryName { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    public string DomainCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public SkillType SkillType { get; set; }
    public string SkillTypeName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompanySpecific { get; set; }

    /// <summary>
    /// Proficiency levels applicable for this skill (e.g., "2,3,4,5,6")
    /// </summary>
    public string? ApplicableLevelsString { get; set; }

    public int EmployeeCount { get; set; }  // How many employees have this skill
}

public class CreateSkillDto
{
    public Guid SubcategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillCategory Category { get; set; }
    public SkillType SkillType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsCompanySpecific { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateSkillDto
{
    public Guid SubcategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillCategory Category { get; set; }
    public SkillType SkillType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompanySpecific { get; set; }
    public List<string>? Tags { get; set; }
}

public class SkillLevelDefinitionDto
{
    public Guid Id { get; set; }
    public ProficiencyLevel Level { get; set; }
    public string LevelName { get; set; } = string.Empty;  // Default SFIA level name
    public string? CustomLevelName { get; set; }  // Custom name if set
    public string DisplayLevelName => CustomLevelName ?? LevelName;  // Used for display
    public string Description { get; set; } = string.Empty;
    public string? Autonomy { get; set; }
    public string? Influence { get; set; }
    public string? Complexity { get; set; }
    public string? BusinessSkills { get; set; }
    public string? Knowledge { get; set; }
    public List<string>? BehavioralIndicators { get; set; }
    public List<string>? EvidenceExamples { get; set; }
}

public class CreateSkillLevelDefinitionDto
{
    public Guid SkillId { get; set; }
    public ProficiencyLevel Level { get; set; }
    public string? CustomLevelName { get; set; }  // Custom name to override default
    public string Description { get; set; } = string.Empty;
    public string? Autonomy { get; set; }
    public string? Influence { get; set; }
    public string? Complexity { get; set; }
    public string? BusinessSkills { get; set; }
    public string? Knowledge { get; set; }
    public List<string>? BehavioralIndicators { get; set; }
    public List<string>? EvidenceExamples { get; set; }
}

public class SkillDropdownDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;  // "Domain > Subcategory > Skill"
    public SkillCategory Category { get; set; }
    public SkillType SkillType { get; set; }
}

// Proficiency Level Definition DTOs
public class ProficiencyLevelDefinitionDto
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Autonomy { get; set; }
    public string? Influence { get; set; }
    public string? Complexity { get; set; }
    public string? Knowledge { get; set; }
    public string? BusinessSkills { get; set; }
    public List<string>? BehavioralIndicators { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProficiencyLevelDefinitionDto
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Autonomy { get; set; }
    public string? Influence { get; set; }
    public string? Complexity { get; set; }
    public string? Knowledge { get; set; }
    public string? BusinessSkills { get; set; }
    public List<string>? BehavioralIndicators { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateProficiencyLevelDefinitionDto
{
    public string LevelName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Autonomy { get; set; }
    public string? Influence { get; set; }
    public string? Complexity { get; set; }
    public string? Knowledge { get; set; }
    public string? BusinessSkills { get; set; }
    public List<string>? BehavioralIndicators { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
