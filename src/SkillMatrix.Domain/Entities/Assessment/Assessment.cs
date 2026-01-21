using SkillMatrix.Domain.Common;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Domain.Entities.Assessment;

/// <summary>
/// Assessment session - a complete evaluation of an employee
/// Can be self-assessment, manager assessment, or test-based
/// </summary>
public class Assessment : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? AssessorId { get; set; }  // For manager/peer assessments
    public AssessmentType Type { get; set; }
    public AssessmentStatus Status { get; set; } = AssessmentStatus.Draft;

    // Metadata
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }

    // For test-based assessments
    public Guid? TestTemplateId { get; set; }
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    public double? Percentage { get; set; }

    // AI Analysis
    public string? AiAnalysis { get; set; }      // JSON: AI's analysis of results
    public string? AiRecommendations { get; set; } // JSON: AI's recommendations

    // Employee feedback (dispute resolution)
    public string? EmployeeFeedback { get; set; }
    public DateTime? FeedbackSubmittedAt { get; set; }
    public string? FeedbackResolution { get; set; }
    public DateTime? FeedbackResolvedAt { get; set; }
    public Guid? FeedbackResolvedBy { get; set; }

    // Navigation
    public Organization.Employee Employee { get; set; } = null!;
    public Organization.Employee? Assessor { get; set; }
    public TestTemplate? TestTemplate { get; set; }
    public ICollection<AssessmentSkillResult> SkillResults { get; set; } = new List<AssessmentSkillResult>();
    public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
}

/// <summary>
/// Skill-level result from an assessment
/// Records the assessed level for each skill evaluated
/// </summary>
public class AssessmentSkillResult : BaseEntity
{
    public Guid AssessmentId { get; set; }
    public Guid SkillId { get; set; }

    // Assessed level
    public ProficiencyLevel AssessedLevel { get; set; }
    public ProficiencyLevel? PreviousLevel { get; set; }

    // Scoring details
    public int? CorrectAnswers { get; set; }
    public int? TotalQuestions { get; set; }
    public double? ScorePercentage { get; set; }

    // Evidence/justification
    public string? Evidence { get; set; }
    public string? AssessorNotes { get; set; }

    // AI explanation
    public string? AiExplanation { get; set; }  // Why AI assessed at this level

    // Navigation
    public Assessment Assessment { get; set; } = null!;
    public Taxonomy.Skill Skill { get; set; } = null!;
}
