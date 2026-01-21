using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Assessment;

public class TestTemplateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public Guid? TargetJobRoleId { get; set; }
    public string? TargetJobRoleName { get; set; }
    public Guid? TargetSkillId { get; set; }
    public string? TargetSkillName { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool IsRandomized { get; set; }
    public int? MaxQuestions { get; set; }
    public bool IsAiGenerated { get; set; }
    public bool RequiresReview { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int SectionCount { get; set; }
    public int QuestionCount { get; set; }
    public List<TestSectionDto> Sections { get; set; } = new();
}

public class TestTemplateListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? TargetJobRoleName { get; set; }
    public string? TargetSkillName { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool IsAiGenerated { get; set; }
    public bool IsActive { get; set; }
    public int SectionCount { get; set; }
    public int QuestionCount { get; set; }
    public int AssessmentCount { get; set; }
}

public class CreateTestTemplateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentType Type { get; set; }
    public Guid? TargetJobRoleId { get; set; }
    public Guid? TargetSkillId { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; } = 70;
    public bool IsRandomized { get; set; }
    public int? MaxQuestions { get; set; }
    public bool RequiresReview { get; set; } = true;
}

public class UpdateTestTemplateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentType Type { get; set; }
    public Guid? TargetJobRoleId { get; set; }
    public Guid? TargetSkillId { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool IsRandomized { get; set; }
    public int? MaxQuestions { get; set; }
    public bool RequiresReview { get; set; }
    public bool IsActive { get; set; }
}

public class TestSectionDto
{
    public Guid Id { get; set; }
    public Guid TestTemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int QuestionCount { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

public class CreateTestSectionDto
{
    public Guid TestTemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int? TimeLimitMinutes { get; set; }
}

public class UpdateTestSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int? TimeLimitMinutes { get; set; }
}
