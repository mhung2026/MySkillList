using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Assessment;

/// <summary>
/// DTO for Assessment session
/// </summary>
public class AssessmentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? AssessorId { get; set; }
    public string? AssessorName { get; set; }
    public AssessmentType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public AssessmentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Test info
    public Guid? TestTemplateId { get; set; }
    public string? TestTemplateTitle { get; set; }
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    public double? Percentage { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? PassingScore { get; set; }

    public DateTime CreatedAt { get; set; }
    public List<AssessmentResponseDto> Responses { get; set; } = new();
}

/// <summary>
/// DTO for Assessment list
/// </summary>
public class AssessmentListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public AssessmentType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public AssessmentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? TestTemplateTitle { get; set; }
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    public double? Percentage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create new Assessment (start taking a test)
/// </summary>
public class StartAssessmentRequest
{
    public Guid EmployeeId { get; set; }
    public Guid TestTemplateId { get; set; }
    public string? Title { get; set; }
}

/// <summary>
/// Response when starting a test
/// </summary>
public class StartAssessmentResponse
{
    public Guid AssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalPoints { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? MustCompleteBy { get; set; }
    public List<TestSectionWithQuestionsDto> Sections { get; set; } = new();
}

/// <summary>
/// Section with questions for test taker
/// </summary>
public class TestSectionWithQuestionsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public List<QuestionForTestDto> Questions { get; set; } = new();
}

/// <summary>
/// Question displayed to test taker (correct answer hidden)
/// </summary>
public class QuestionForTestDto
{
    public Guid Id { get; set; }
    public int QuestionNumber { get; set; }
    public QuestionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string? MediaUrl { get; set; }
    public int Points { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public List<OptionForTestDto> Options { get; set; } = new();
}

/// <summary>
/// Option for question (isCorrect not displayed)
/// </summary>
public class OptionForTestDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for response (test taker's answer)
/// </summary>
public class AssessmentResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string? TextResponse { get; set; }
    public string? CodeResponse { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; }
    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
    public int MaxPoints { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public int? TimeSpentSeconds { get; set; }
}

/// <summary>
/// Request to submit answer
/// </summary>
public class SubmitAnswerRequest
{
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; }
    public string? TextResponse { get; set; }
    public string? CodeResponse { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; }
    public int? TimeSpentSeconds { get; set; }
}

/// <summary>
/// Response after submitting answer
/// </summary>
public class SubmitAnswerResponse
{
    public bool Success { get; set; }
    public Guid ResponseId { get; set; }
    public bool? IsCorrect { get; set; }  // null if not yet graded (essay/coding)
    public int? PointsAwarded { get; set; }
    public string? Feedback { get; set; }  // Immediate feedback (if available)
}

/// <summary>
/// Request to submit test (complete test)
/// </summary>
public class SubmitAssessmentRequest
{
    public Guid AssessmentId { get; set; }
}

/// <summary>
/// Response when test submission is complete
/// </summary>
public class AssessmentResultDto
{
    public Guid AssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public AssessmentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // Scores
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public int PassingScore { get; set; }

    // Time
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int TotalTimeMinutes { get; set; }

    // Statistics
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int UnansweredQuestions { get; set; }
    public int PendingReviewQuestions { get; set; }

    // Breakdown by skill
    public List<SkillResultDto> SkillResults { get; set; } = new();

    // Detailed responses (for review)
    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}

/// <summary>
/// Result by each skill
/// </summary>
public class SkillResultDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Result for each question (for review)
/// </summary>
public class QuestionResultDto
{
    public Guid QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public QuestionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int Points { get; set; }

    // User's answer
    public string? UserAnswer { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; }

    // Correct answer (shown after submit)
    public string? CorrectAnswer { get; set; }
    public List<Guid>? CorrectOptionIds { get; set; }

    // Scoring
    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
    public string? Explanation { get; set; }

    // Options with correct marking
    public List<OptionResultDto> Options { get; set; } = new();
}

/// <summary>
/// Option result with correct answer marking
/// </summary>
public class OptionResultDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool WasSelected { get; set; }
    public string? Explanation { get; set; }
}
