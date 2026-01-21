using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Assessment;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionTitle { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public ProficiencyLevel TargetLevel { get; set; }
    public string TargetLevelName { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string? MediaUrl { get; set; }
    public int Points { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public string DifficultyName { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public string? AiPromptUsed { get; set; }
    public bool IsActive { get; set; }
    public List<string>? Tags { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
    public string? GradingRubric { get; set; }
}

public class QuestionListDto
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public ProficiencyLevel TargetLevel { get; set; }
    public string TargetLevelName { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Points { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public string DifficultyName { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public bool IsActive { get; set; }
    public int OptionCount { get; set; }
}

public class CreateQuestionDto
{
    public Guid? SectionId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel TargetLevel { get; set; }
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string? MediaUrl { get; set; }
    public int Points { get; set; } = 1;
    public int? TimeLimitSeconds { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public List<string>? Tags { get; set; }
    public string? GradingRubric { get; set; }
    public List<CreateQuestionOptionDto> Options { get; set; } = new();
}

public class UpdateQuestionDto
{
    public Guid? SectionId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel TargetLevel { get; set; }
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string? MediaUrl { get; set; }
    public int Points { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsActive { get; set; }
    public List<string>? Tags { get; set; }
    public string? GradingRubric { get; set; }
    public List<CreateQuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
    public string? Explanation { get; set; }
}

public class CreateQuestionOptionDto
{
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
    public string? Explanation { get; set; }
}

public class QuestionFilterRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? SkillId { get; set; }
    public Guid? SectionId { get; set; }
    public ProficiencyLevel? TargetLevel { get; set; }
    public QuestionType? Type { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public bool? IsAiGenerated { get; set; }
    public bool IncludeInactive { get; set; }
}
