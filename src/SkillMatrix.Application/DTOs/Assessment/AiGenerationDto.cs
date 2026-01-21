using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Assessment;

/// <summary>
/// Request để AI generate câu hỏi
/// </summary>
public class AiGenerateQuestionsRequest
{
    /// <summary>
    /// Skill ID cần generate câu hỏi
    /// </summary>
    public Guid SkillId { get; set; }

    /// <summary>
    /// Level cần đánh giá (1-7)
    /// </summary>
    public ProficiencyLevel TargetLevel { get; set; }

    /// <summary>
    /// Số lượng câu hỏi cần generate
    /// </summary>
    public int QuestionCount { get; set; } = 5;

    /// <summary>
    /// Loại câu hỏi cần generate
    /// </summary>
    public List<QuestionType> QuestionTypes { get; set; } = new() { QuestionType.MultipleChoice };

    /// <summary>
    /// Độ khó mong muốn
    /// </summary>
    public DifficultyLevel? Difficulty { get; set; }

    /// <summary>
    /// Ngôn ngữ output (vi, en)
    /// </summary>
    public string Language { get; set; } = "vi";

    /// <summary>
    /// Context bổ sung cho AI (vd: focus vào .NET Core 8)
    /// </summary>
    public string? AdditionalContext { get; set; }

    /// <summary>
    /// Section ID nếu muốn add vào section cụ thể
    /// </summary>
    public Guid? SectionId { get; set; }
}

/// <summary>
/// Response từ AI service
/// </summary>
public class AiGenerateQuestionsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public List<AiGeneratedQuestion> Questions { get; set; } = new();
    public AiGenerationMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Câu hỏi được AI generate
/// </summary>
public class AiGeneratedQuestion
{
    public string Content { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public QuestionType Type { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int SuggestedPoints { get; set; }
    public int? SuggestedTimeSeconds { get; set; }
    public List<AiGeneratedOption> Options { get; set; } = new();
    public string? Explanation { get; set; }
    public List<string> Tags { get; set; } = new();

    // For non-multiple choice
    public string? ExpectedAnswer { get; set; }
    public string? GradingRubric { get; set; }
}

/// <summary>
/// Option cho câu hỏi trắc nghiệm
/// </summary>
public class AiGeneratedOption
{
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// Metadata về quá trình generate
/// </summary>
public class AiGenerationMetadata
{
    public string Model { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public double GenerationTimeMs { get; set; }
    public string PromptUsed { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Request để AI phân tích và đề xuất skill gaps
/// </summary>
public class AiAnalyzeSkillGapRequest
{
    public Guid EmployeeId { get; set; }
    public Guid? JobRoleId { get; set; }
    public List<EmployeeSkillSnapshot> CurrentSkills { get; set; } = new();
}

public class EmployeeSkillSnapshot
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public ProficiencyLevel CurrentLevel { get; set; }
    public ProficiencyLevel? RequiredLevel { get; set; }
}

/// <summary>
/// Response phân tích từ AI
/// </summary>
public class AiAnalyzeSkillGapResponse
{
    public bool Success { get; set; }
    public List<AiSkillGapAnalysis> Gaps { get; set; } = new();
    public List<AiLearningRecommendation> Recommendations { get; set; } = new();
    public string? OverallAssessment { get; set; }
}

public class AiSkillGapAnalysis
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public ProficiencyLevel CurrentLevel { get; set; }
    public ProficiencyLevel RequiredLevel { get; set; }
    public int GapSize { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class AiLearningRecommendation
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;  // Course, Book, Project, etc.
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public int? EstimatedHours { get; set; }
    public string Rationale { get; set; } = string.Empty;
}

/// <summary>
/// Request để AI chấm bài tự luận/coding
/// </summary>
public class AiGradeAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public string? ExpectedAnswer { get; set; }
    public string? GradingRubric { get; set; }
    public string StudentAnswer { get; set; } = string.Empty;
    public int MaxPoints { get; set; }
}

public class AiGradeAnswerResponse
{
    public bool Success { get; set; }
    public int PointsAwarded { get; set; }
    public int MaxPoints { get; set; }
    public double Percentage { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public List<string> StrengthPoints { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public string? DetailedAnalysis { get; set; }
}
