using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Assessment;

/// <summary>
/// Request for AI to generate questions
/// </summary>
public class AiGenerateQuestionsRequest
{
    /// <summary>
    /// Skill ID to generate questions for (optional)
    /// </summary>
    public Guid? SkillId { get; set; }

    /// <summary>
    /// Skill name - used when SkillId is not provided (optional)
    /// </summary>
    public string? SkillName { get; set; }

    /// <summary>
    /// Skill code (e.g., ACIN, PROG) - optional
    /// </summary>
    public string? SkillCode { get; set; }

    /// <summary>
    /// Target level to assess (1-7) - optional
    /// </summary>
    public ProficiencyLevel? TargetLevel { get; set; }

    /// <summary>
    /// Number of questions to generate
    /// </summary>
    public int QuestionCount { get; set; } = 5;

    /// <summary>
    /// Assessment type: Quiz, CodingTest, CaseStudy, RoleBasedTest, SituationalJudgment
    /// </summary>
    public AssessmentType AssessmentType { get; set; } = AssessmentType.Quiz;

    /// <summary>
    /// Question types to generate (required): MultipleChoice, MultipleAnswer, TrueFalse, etc.
    /// </summary>
    public List<QuestionType> QuestionTypes { get; set; } = new();

    /// <summary>
    /// Desired difficulty level (optional)
    /// </summary>
    public DifficultyLevel? Difficulty { get; set; }

    /// <summary>
    /// Output language (en, vi)
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Additional context for AI (e.g., focus on .NET Core 8)
    /// </summary>
    public string? AdditionalContext { get; set; }

    /// <summary>
    /// Job role - used for Role-based Test
    /// </summary>
    public string? JobRole { get; set; }

    /// <summary>
    /// Section ID if adding to a specific section
    /// </summary>
    public Guid? SectionId { get; set; }
}

/// <summary>
/// Response from AI service
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
/// AI-generated question
/// </summary>
public class AiGeneratedQuestion
{
    /// <summary>
    /// Question content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Assessment type
    /// </summary>
    public AssessmentType AssessmentType { get; set; }

    /// <summary>
    /// Specific question type within assessment
    /// </summary>
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// Difficulty level (optional)
    /// </summary>
    public DifficultyLevel? Difficulty { get; set; }

    /// <summary>
    /// Suggested target level (optional)
    /// </summary>
    public ProficiencyLevel? TargetLevel { get; set; }

    /// <summary>
    /// Related skill ID (optional)
    /// </summary>
    public Guid? SkillId { get; set; }

    /// <summary>
    /// Related skill name (optional)
    /// </summary>
    public string? SkillName { get; set; }

    /// <summary>
    /// Suggested points
    /// </summary>
    public int SuggestedPoints { get; set; }

    /// <summary>
    /// Suggested time (seconds)
    /// </summary>
    public int? SuggestedTimeSeconds { get; set; }

    /// <summary>
    /// Tags for the question
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Answer explanation
    /// </summary>
    public string? Explanation { get; set; }

    // === For Quiz (multiple choice) ===
    /// <summary>
    /// Options for multiple choice questions
    /// </summary>
    public List<AiGeneratedOption> Options { get; set; } = new();

    // === For Coding Test ===
    /// <summary>
    /// Code snippet/template
    /// </summary>
    public string? CodeSnippet { get; set; }

    /// <summary>
    /// Expected output
    /// </summary>
    public string? ExpectedOutput { get; set; }

    /// <summary>
    /// Test cases for coding
    /// </summary>
    public List<AiTestCase> TestCases { get; set; } = new();

    // === For Case Study ===
    /// <summary>
    /// Detailed scenario description
    /// </summary>
    public string? Scenario { get; set; }

    /// <summary>
    /// Additional documents
    /// </summary>
    public List<string> Documents { get; set; } = new();

    // === For Role-based Test ===
    /// <summary>
    /// Role context
    /// </summary>
    public string? RoleContext { get; set; }

    /// <summary>
    /// Role responsibilities
    /// </summary>
    public List<string> Responsibilities { get; set; } = new();

    // === For SJT ===
    /// <summary>
    /// SJT situation description
    /// </summary>
    public string? Situation { get; set; }

    /// <summary>
    /// Response options for SJT
    /// </summary>
    public List<AiSjtResponseOption> ResponseOptions { get; set; } = new();

    // === Common for essay/open-ended ===
    /// <summary>
    /// Expected answer
    /// </summary>
    public string? ExpectedAnswer { get; set; }

    /// <summary>
    /// Grading rubric
    /// </summary>
    public string? GradingRubric { get; set; }
}

/// <summary>
/// Option for multiple choice questions
/// </summary>
public class AiGeneratedOption
{
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// Test case for coding challenge
/// </summary>
public class AiTestCase
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsHidden { get; set; }  // Hidden test cases for grading
}

/// <summary>
/// Response option for SJT
/// </summary>
public class AiSjtResponseOption
{
    public string Content { get; set; } = string.Empty;
    public SjtEffectiveness Effectiveness { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// Metadata about the generation process
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
/// Request for AI to analyze and recommend skill gaps
/// </summary>
public class AiAnalyzeSkillGapRequest
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? JobRoleId { get; set; }
    public string? JobRoleName { get; set; }
    public List<EmployeeSkillSnapshot> CurrentSkills { get; set; } = new();
}

public class EmployeeSkillSnapshot
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public ProficiencyLevel CurrentLevel { get; set; }
    public ProficiencyLevel? RequiredLevel { get; set; }
}

/// <summary>
/// AI analysis response
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
/// Request for AI to grade essay/coding answers
/// </summary>
public class AiGradeAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public string? ExpectedAnswer { get; set; }
    public string? GradingRubric { get; set; }
    public string SubmittedAnswer { get; set; } = string.Empty;
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
