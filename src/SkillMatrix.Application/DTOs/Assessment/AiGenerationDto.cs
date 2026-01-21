using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Assessment;

/// <summary>
/// Request để AI generate câu hỏi
/// </summary>
public class AiGenerateQuestionsRequest
{
    /// <summary>
    /// Skill ID cần generate câu hỏi (optional)
    /// </summary>
    public Guid? SkillId { get; set; }

    /// <summary>
    /// Skill name - dùng khi không có SkillId (optional)
    /// </summary>
    public string? SkillName { get; set; }

    /// <summary>
    /// Level cần đánh giá (1-7) - optional
    /// </summary>
    public ProficiencyLevel? TargetLevel { get; set; }

    /// <summary>
    /// Số lượng câu hỏi cần generate
    /// </summary>
    public int QuestionCount { get; set; } = 5;

    /// <summary>
    /// Loại assessment: Quiz, CodingTest, CaseStudy, RoleBasedTest, SituationalJudgment
    /// </summary>
    public AssessmentType AssessmentType { get; set; } = AssessmentType.Quiz;

    /// <summary>
    /// Độ khó mong muốn (optional)
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
    /// Job role - dùng cho Role-based Test
    /// </summary>
    public string? JobRole { get; set; }

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
    public AiGenerationMetadata? Metadata { get; set; }
}

/// <summary>
/// Câu hỏi được AI generate
/// </summary>
public class AiGeneratedQuestion
{
    /// <summary>
    /// Nội dung câu hỏi
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Loại assessment
    /// </summary>
    public AssessmentType AssessmentType { get; set; }

    /// <summary>
    /// Loại câu hỏi cụ thể trong assessment
    /// </summary>
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// Độ khó (optional)
    /// </summary>
    public DifficultyLevel? Difficulty { get; set; }

    /// <summary>
    /// Level gợi ý phù hợp (optional)
    /// </summary>
    public ProficiencyLevel? TargetLevel { get; set; }

    /// <summary>
    /// Skill ID liên quan (optional)
    /// </summary>
    public Guid? SkillId { get; set; }

    /// <summary>
    /// Skill name liên quan (optional)
    /// </summary>
    public string? SkillName { get; set; }

    /// <summary>
    /// Điểm gợi ý
    /// </summary>
    public int SuggestedPoints { get; set; }

    /// <summary>
    /// Thời gian gợi ý (giây)
    /// </summary>
    public int? SuggestedTimeSeconds { get; set; }

    /// <summary>
    /// Tags cho câu hỏi
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Giải thích đáp án
    /// </summary>
    public string? Explanation { get; set; }

    // === Cho Quiz (trắc nghiệm) ===
    /// <summary>
    /// Các lựa chọn cho câu trắc nghiệm
    /// </summary>
    public List<AiGeneratedOption> Options { get; set; } = new();

    // === Cho Coding Test ===
    /// <summary>
    /// Code snippet/template
    /// </summary>
    public string? CodeSnippet { get; set; }

    /// <summary>
    /// Output mong đợi
    /// </summary>
    public string? ExpectedOutput { get; set; }

    /// <summary>
    /// Test cases cho coding
    /// </summary>
    public List<AiTestCase> TestCases { get; set; } = new();

    // === Cho Case Study ===
    /// <summary>
    /// Mô tả tình huống chi tiết
    /// </summary>
    public string? Scenario { get; set; }

    /// <summary>
    /// Tài liệu bổ sung
    /// </summary>
    public List<string> Documents { get; set; } = new();

    // === Cho Role-based Test ===
    /// <summary>
    /// Context về vai trò
    /// </summary>
    public string? RoleContext { get; set; }

    /// <summary>
    /// Trách nhiệm của vai trò
    /// </summary>
    public List<string> Responsibilities { get; set; } = new();

    // === Cho SJT ===
    /// <summary>
    /// Mô tả tình huống SJT
    /// </summary>
    public string? Situation { get; set; }

    /// <summary>
    /// Các phương án xử lý cho SJT
    /// </summary>
    public List<AiSjtResponseOption> ResponseOptions { get; set; } = new();

    // === Chung cho tự luận ===
    /// <summary>
    /// Câu trả lời mong đợi
    /// </summary>
    public string? ExpectedAnswer { get; set; }

    /// <summary>
    /// Rubric chấm điểm
    /// </summary>
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
/// Test case cho coding challenge
/// </summary>
public class AiTestCase
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsHidden { get; set; }  // Hidden test cases for grading
}

/// <summary>
/// Response option cho SJT
/// </summary>
public class AiSjtResponseOption
{
    public string Content { get; set; } = string.Empty;
    public SjtEffectiveness Effectiveness { get; set; }
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
    public string? PromptUsed { get; set; }
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
