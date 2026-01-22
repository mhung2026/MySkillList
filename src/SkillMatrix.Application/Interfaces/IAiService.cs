using SkillMatrix.Application.DTOs.Assessment;

namespace SkillMatrix.Application.Interfaces;

/// <summary>
/// Interface for AI Service - will be implemented by real AI or mock
/// </summary>
public interface IAiQuestionGeneratorService
{
    /// <summary>
    /// Generate questions based on skill and level
    /// </summary>
    Task<AiGenerateQuestionsResponse> GenerateQuestionsAsync(AiGenerateQuestionsRequest request);

    /// <summary>
    /// Grade essay/coding answer
    /// </summary>
    Task<AiGradeAnswerResponse> GradeAnswerAsync(AiGradeAnswerRequest request);
}

/// <summary>
/// Interface for AI skill gap analysis
/// </summary>
public interface IAiSkillAnalyzerService
{
    /// <summary>
    /// Analyze employee skill gaps
    /// </summary>
    Task<AiAnalyzeSkillGapResponse> AnalyzeSkillGapsAsync(AiAnalyzeSkillGapRequest request);
}

/// <summary>
/// Options to configure AI service
/// </summary>
public class AiServiceOptions
{
    public const string SectionName = "AiService";

    /// <summary>
    /// Use mock service or real AI
    /// </summary>
    public bool UseMock { get; set; } = true;

    /// <summary>
    /// API Key for real AI service
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Base URL for AI API
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Model name (gpt-4, claude-3, etc.)
    /// </summary>
    public string ModelName { get; set; } = "mock-model";

    /// <summary>
    /// Timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
