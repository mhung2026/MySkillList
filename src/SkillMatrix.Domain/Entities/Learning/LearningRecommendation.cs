using SkillMatrix.Domain.Common;

namespace SkillMatrix.Domain.Entities.Learning;

/// <summary>
/// AI-generated learning recommendations for skill gaps
/// Stores Coursera courses, projects, mentorship suggestions
/// </summary>
public class LearningRecommendation : BaseEntity
{
    public Guid SkillGapId { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;

    // Recommendation details
    public string RecommendationType { get; set; } = string.Empty;  // Course, Project, Mentorship, Book
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }  // Coursera course URL, etc.
    public int? EstimatedHours { get; set; }
    public string Rationale { get; set; } = string.Empty;  // Why this recommendation

    // Tracking
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }

    // AI metadata
    public string? AiProvider { get; set; }  // OpenAI, Gemini, etc.
    public DateTime GeneratedAt { get; set; }

    // Navigation
    public SkillGap SkillGap { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}
