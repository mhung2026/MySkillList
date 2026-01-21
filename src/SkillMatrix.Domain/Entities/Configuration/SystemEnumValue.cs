using SkillMatrix.Domain.Common;

namespace SkillMatrix.Domain.Entities.Configuration;

/// <summary>
/// Dynamic enum value that can be configured from admin
/// Replaces hard-coded enums for flexibility
/// </summary>
public class SystemEnumValue : BaseEntity
{
    /// <summary>
    /// Type of enum (e.g., "SkillCategory", "AssessmentType", "QuestionType")
    /// </summary>
    public string EnumType { get; set; } = string.Empty;

    /// <summary>
    /// Numeric value (1, 2, 3...) - used for storage and comparison
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Code/key for programmatic use (e.g., "Technical", "Quiz", "MultipleChoice")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name in English
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name in Vietnamese
    /// </summary>
    public string? NameVi { get; set; }

    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Description in Vietnamese
    /// </summary>
    public string? DescriptionVi { get; set; }

    /// <summary>
    /// Color code for UI display (e.g., "#52c41a", "blue")
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icon name for UI (e.g., "CheckCircleOutlined", "CodeOutlined")
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Display order within the enum type
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this value is currently active/usable
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// System values cannot be deleted (core values required for system operation)
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// JSON metadata for additional type-specific information
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Supported enum types for configuration
/// </summary>
public static class SystemEnumTypes
{
    public const string SkillCategory = "SkillCategory";
    public const string SkillType = "SkillType";
    public const string AssessmentType = "AssessmentType";
    public const string AssessmentStatus = "AssessmentStatus";
    public const string QuestionType = "QuestionType";
    public const string DifficultyLevel = "DifficultyLevel";
    public const string GapPriority = "GapPriority";
    public const string LearningResourceType = "LearningResourceType";
    public const string LearningPathStatus = "LearningPathStatus";
    public const string EmploymentStatus = "EmploymentStatus";
    public const string UserRole = "UserRole";
    public const string SjtEffectiveness = "SjtEffectiveness";

    public static readonly string[] All = new[]
    {
        SkillCategory,
        SkillType,
        AssessmentType,
        AssessmentStatus,
        QuestionType,
        DifficultyLevel,
        GapPriority,
        LearningResourceType,
        LearningPathStatus,
        EmploymentStatus,
        UserRole,
        SjtEffectiveness
    };
}
