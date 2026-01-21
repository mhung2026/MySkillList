namespace SkillMatrix.Application.DTOs.Taxonomy;

public class SkillSubcategoryDto
{
    public Guid Id { get; set; }
    public Guid SkillDomainId { get; set; }
    public string SkillDomainName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Stats
    public int SkillCount { get; set; }
}

public class SkillSubcategoryListDto
{
    public Guid Id { get; set; }
    public Guid SkillDomainId { get; set; }
    public string SkillDomainCode { get; set; } = string.Empty;
    public string SkillDomainName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int SkillCount { get; set; }
}

public class CreateSkillSubcategoryDto
{
    public Guid SkillDomainId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateSkillSubcategoryDto
{
    public Guid SkillDomainId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class SkillSubcategoryDropdownDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;  // "Domain > Subcategory"
}
