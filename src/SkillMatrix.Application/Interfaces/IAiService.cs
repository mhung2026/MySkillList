using SkillMatrix.Application.DTOs.Assessment;

namespace SkillMatrix.Application.Interfaces;

/// <summary>
/// Interface cho AI Service - sẽ được implement bởi real AI hoặc mock
/// </summary>
public interface IAiQuestionGeneratorService
{
    /// <summary>
    /// Generate câu hỏi dựa trên skill và level
    /// </summary>
    Task<AiGenerateQuestionsResponse> GenerateQuestionsAsync(AiGenerateQuestionsRequest request);

    /// <summary>
    /// Chấm điểm câu trả lời tự luận/coding
    /// </summary>
    Task<AiGradeAnswerResponse> GradeAnswerAsync(AiGradeAnswerRequest request);
}

/// <summary>
/// Interface cho AI phân tích skill gaps
/// </summary>
public interface IAiSkillAnalyzerService
{
    /// <summary>
    /// Phân tích skill gaps của employee
    /// </summary>
    Task<AiAnalyzeSkillGapResponse> AnalyzeSkillGapsAsync(AiAnalyzeSkillGapRequest request);
}

/// <summary>
/// Options để configure AI service
/// </summary>
public class AiServiceOptions
{
    public const string SectionName = "AiService";

    /// <summary>
    /// Sử dụng mock service hay real AI
    /// </summary>
    public bool UseMock { get; set; } = true;

    /// <summary>
    /// API Key cho real AI service
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Base URL cho AI API
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
