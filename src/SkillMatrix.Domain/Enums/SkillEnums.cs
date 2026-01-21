namespace SkillMatrix.Domain.Enums;

/// <summary>
/// Skill Category - Top level classification (extensible)
/// Maps to SFIA categories but allows custom additions
/// </summary>
public enum SkillCategory
{
    // Technical Skills
    Technical = 1,

    // Soft/Professional Skills
    Professional = 2,

    // Domain/Business Knowledge
    Domain = 3,

    // Leadership & Management
    Leadership = 4,

    // Tools & Platforms
    Tools = 5
}

/// <summary>
/// SFIA-based proficiency levels (1-7)
/// Level 0 = No knowledge/experience
/// </summary>
public enum ProficiencyLevel
{
    None = 0,
    Follow = 1,        // Follows instructions, learning
    Assist = 2,        // Assists others, developing skills
    Apply = 3,         // Applies skills independently
    Enable = 4,        // Enables others, ensures quality
    EnsureAdvise = 5,  // Ensures/Advises at organizational level
    Initiate = 6,      // Initiates, influences strategic direction
    SetStrategy = 7    // Sets strategy, leads industry
}

/// <summary>
/// Skill type for T-shaped model classification
/// </summary>
public enum SkillType
{
    Core = 1,      // Everyone needs (Git, Communication, etc.)
    Specialty = 2, // Role-specific deep skills
    Adjacent = 3   // Nice to have, cross-functional
}

/// <summary>
/// Assessment types
/// </summary>
public enum AssessmentType
{
    SelfAssessment = 1,
    ManagerAssessment = 2,
    PeerAssessment = 3,
    Quiz = 4,
    CodingTest = 5,
    CaseStudy = 6,
    RoleBasedTest = 7,
    SituationalJudgment = 8  // SJT
}

/// <summary>
/// Assessment status
/// </summary>
public enum AssessmentStatus
{
    Draft = 1,
    Pending = 2,
    InProgress = 3,
    Completed = 4,
    Reviewed = 5,
    Disputed = 6,    // Employee feedback/dispute
    Resolved = 7
}

/// <summary>
/// Question types for assessments
/// </summary>
public enum QuestionType
{
    MultipleChoice = 1,
    MultipleAnswer = 2,
    TrueFalse = 3,
    ShortAnswer = 4,
    LongAnswer = 5,
    CodingChallenge = 6,
    CaseStudy = 7,
    BehavioralAnchored = 8  // BARS - Level descriptions
}

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    Employee = 1,
    TeamLead = 2,
    Manager = 3,
    Admin = 4
}

/// <summary>
/// Employment status
/// </summary>
public enum EmploymentStatus
{
    Active = 1,
    OnLeave = 2,
    Resigned = 3,
    Terminated = 4
}

/// <summary>
/// Learning resource types
/// </summary>
public enum LearningResourceType
{
    Course = 1,
    Book = 2,
    Video = 3,
    Article = 4,
    Workshop = 5,
    Certification = 6,
    Project = 7,      // Hands-on project
    Mentorship = 8,
    Seminar = 9       // Internal knowledge sharing
}

/// <summary>
/// Learning path status
/// </summary>
public enum LearningPathStatus
{
    Suggested = 1,    // AI suggested
    Approved = 2,     // Manager approved
    InProgress = 3,
    Completed = 4,
    Paused = 5,
    Cancelled = 6
}

/// <summary>
/// Gap priority level
/// </summary>
public enum GapPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
