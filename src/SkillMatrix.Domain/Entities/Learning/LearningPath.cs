using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Learning;

/// <summary>
/// Learning resource - courses, books, certifications, etc.
/// </summary>
public class LearningResource : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LearningResourceType Type { get; set; }
    public string? Url { get; set; }
    public string? Provider { get; set; }  // Udemy, Coursera, internal, etc.

    // Duration/Effort
    public int? EstimatedHours { get; set; }
    public DifficultyLevel Difficulty { get; set; }

    // Metadata
    public bool IsInternal { get; set; }  // Company-provided vs external
    public bool IsFree { get; set; }
    public decimal? Cost { get; set; }
    public string? Tags { get; set; }  // JSON array
    public bool IsActive { get; set; } = true;

    // AI generated/suggested
    public bool IsAiSuggested { get; set; }
    public double? AiRelevanceScore { get; set; }

    // Navigation
    public ICollection<LearningResourceSkill> TargetSkills { get; set; } = new List<LearningResourceSkill>();
    public ICollection<LearningPathItem> PathItems { get; set; } = new List<LearningPathItem>();
}

/// <summary>
/// Skills that a learning resource develops
/// </summary>
public class LearningResourceSkill : BaseEntity
{
    public Guid LearningResourceId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel FromLevel { get; set; }  // Appropriate starting level
    public ProficiencyLevel ToLevel { get; set; }    // Expected level after completion

    // Navigation
    public LearningResource LearningResource { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}

/// <summary>
/// Personalized learning path for an employee
/// AI-generated and manager-approved
/// </summary>
public class EmployeeLearningPath : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? SkillGapId { get; set; }  // If addressing a specific gap
    public Guid? TargetSkillId { get; set; }

    // Path details
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProficiencyLevel? CurrentLevel { get; set; }
    public ProficiencyLevel TargetLevel { get; set; }
    public LearningPathStatus Status { get; set; } = LearningPathStatus.Suggested;

    // Timeline
    public DateTime? TargetCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public int? EstimatedTotalHours { get; set; }

    // AI Generation
    public bool IsAiGenerated { get; set; }
    public string? AiRationale { get; set; }  // Why AI suggested this path
    public DateTime? AiGeneratedAt { get; set; }

    // Approval workflow
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApprovalNotes { get; set; }

    // Progress
    public int ProgressPercentage { get; set; }
    public DateTime? LastActivityAt { get; set; }

    // Navigation
    public Organization.Employee Employee { get; set; } = null!;
    public SkillGap? SkillGap { get; set; }
    public Taxonomy.Skill? TargetSkill { get; set; }
    public ICollection<LearningPathItem> Items { get; set; } = new List<LearningPathItem>();
}

/// <summary>
/// Individual item/step in a learning path
/// </summary>
public class LearningPathItem : BaseEntity
{
    public Guid LearningPathId { get; set; }
    public Guid? LearningResourceId { get; set; }

    // Item details
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LearningResourceType ItemType { get; set; }
    public int DisplayOrder { get; set; }
    public int? EstimatedHours { get; set; }

    // AI-generated fields
    public ProficiencyLevel? TargetLevelAfter { get; set; }  // Expected level after completing this item
    public string? SuccessCriteria { get; set; }  // How to measure success
    public string? ExternalUrl { get; set; }  // Link to external resource (e.g., Coursera)

    // Progress
    public LearningItemStatus Status { get; set; } = LearningItemStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ActualHoursSpent { get; set; }

    // Outcome
    public string? Notes { get; set; }  // Employee notes
    public string? Outcome { get; set; }  // What was learned/achieved

    // Navigation
    public EmployeeLearningPath LearningPath { get; set; } = null!;
    public LearningResource? LearningResource { get; set; }
}

public enum LearningItemStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    Skipped = 4
}
