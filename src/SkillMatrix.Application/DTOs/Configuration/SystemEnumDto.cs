namespace SkillMatrix.Application.DTOs.Configuration;

/// <summary>
/// DTO for SystemEnumValue
/// </summary>
public class SystemEnumValueDto
{
    public Guid Id { get; set; }
    public string EnumType { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameVi { get; set; }
    public string? Description { get; set; }
    public string? DescriptionVi { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO for creating a new enum value
/// </summary>
public class CreateSystemEnumValueDto
{
    public string EnumType { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameVi { get; set; }
    public string? Description { get; set; }
    public string? DescriptionVi { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO for updating an enum value
/// </summary>
public class UpdateSystemEnumValueDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameVi { get; set; }
    public string? Description { get; set; }
    public string? DescriptionVi { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Response DTO for enum type with all its values
/// </summary>
public class EnumTypeDto
{
    public string EnumType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ValueCount { get; set; }
    public List<SystemEnumValueDto> Values { get; set; } = new();
}

/// <summary>
/// Simple enum value for dropdowns
/// </summary>
public class EnumDropdownItemDto
{
    public int Value { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? LabelVi { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// Request to reorder enum values
/// </summary>
public class ReorderEnumValuesDto
{
    public string EnumType { get; set; } = string.Empty;
    public List<Guid> OrderedIds { get; set; } = new();
}
