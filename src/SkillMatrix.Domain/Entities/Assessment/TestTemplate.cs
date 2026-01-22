using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Assessment;

/// <summary>
/// Test template - reusable assessment template
/// Can be role-based, skill-based, or custom
/// </summary>
public class TestTemplate : VersionedEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentType Type { get; set; }

    // Targeting
    public Guid? TargetJobRoleId { get; set; }  // For role-based tests
    public Guid? TargetSkillId { get; set; }    // For single skill tests

    // Configuration
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }  // Percentage
    public bool IsRandomized { get; set; }
    public int? MaxQuestions { get; set; }  // If randomized, how many to pick
    public bool IsAiGenerated { get; set; }
    public bool RequiresReview { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Organization.JobRole? TargetJobRole { get; set; }
    public Taxonomy.Skill? TargetSkill { get; set; }
    public ICollection<TestSection> Sections { get; set; } = new List<TestSection>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
}

/// <summary>
/// Section within a test (e.g., "Programming Basics", "Case Study")
/// </summary>
public class TestSection : BaseEntity
{
    public Guid TestTemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int? TimeLimitMinutes { get; set; }

    // Navigation
    public TestTemplate TestTemplate { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

/// <summary>
/// Question entity - supports multiple question types
/// </summary>
public class Question : VersionedEntity
{
    public Guid? SectionId { get; set; }
    public Guid? SkillId { get; set; }  // Optional - can generate questions without specific skill
    public ProficiencyLevel TargetLevel { get; set; }  // What level this question tests

    // Content
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;  // The question text/prompt
    public string? CodeSnippet { get; set; }  // For coding questions
    public string? MediaUrl { get; set; }      // Image, diagram, etc.

    // Scoring
    public int Points { get; set; } = 1;
    public int? TimeLimitSeconds { get; set; }
    public string? GradingRubric { get; set; }  // JSON: For manual grading

    // Metadata
    public DifficultyLevel Difficulty { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? AiPromptUsed { get; set; }  // For regeneration/improvement
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }  // JSON array

    // Navigation
    public TestSection? Section { get; set; }
    public Taxonomy.Skill? Skill { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
}

/// <summary>
/// Answer options for multiple choice questions
/// </summary>
public class QuestionOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
    public string? Explanation { get; set; }  // Why this is correct/incorrect

    // Navigation
    public Question Question { get; set; } = null!;
}

/// <summary>
/// Employee's response to a question
/// </summary>
public class AssessmentResponse : BaseEntity
{
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; }

    // Response content
    public string? TextResponse { get; set; }      // For text answers
    public string? CodeResponse { get; set; }      // For coding answers
    public string? SelectedOptions { get; set; }   // JSON array of option IDs

    // Scoring
    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public int? TimeSpentSeconds { get; set; }

    // For manual/AI grading
    public string? AiGrading { get; set; }         // JSON: AI's grading with explanation
    public string? ManualGrading { get; set; }     // JSON: Manual grading notes
    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }

    // Navigation
    public Assessment Assessment { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
