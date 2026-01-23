using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.Services.AI;

/// <summary>
/// Real AI service implementation calling Python Gemini API
/// </summary>
public class GeminiAiQuestionGeneratorService : IAiQuestionGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiAiQuestionGeneratorService> _logger;
    private readonly AiServiceOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public GeminiAiQuestionGeneratorService(
        HttpClient httpClient,
        ILogger<GeminiAiQuestionGeneratorService> logger,
        IOptions<AiServiceOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<AiGenerateQuestionsResponse> GenerateQuestionsAsync(AiGenerateQuestionsRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Generating {Count} questions for skill {SkillId}/{SkillName}",
                request.QuestionCount,
                request.SkillId,
                request.SkillName);

            // 1. Map C# request to Python API V2 request
            var pythonRequest = MapToPythonRequest(request);

            // 2. Call Python API
            var endpoint = $"{_options.BaseUrl}/api/v2/generate-questions";
            _logger.LogDebug("Calling Python API: {Endpoint}", endpoint);

            var response = await _httpClient.PostAsJsonAsync(endpoint, pythonRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Python API returned error {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);

                return new AiGenerateQuestionsResponse
                {
                    Success = false,
                    Error = $"AI service error: {response.StatusCode}",
                    Message = errorContent
                };
            }

            var pythonResponse = await response.Content.ReadFromJsonAsync<PythonApiResponse>(_jsonOptions);

            if (pythonResponse?.Questions == null || !pythonResponse.Questions.Any())
            {
                _logger.LogWarning("Python API returned no questions");
                return new AiGenerateQuestionsResponse
                {
                    Success = false,
                    Message = "No questions generated"
                };
            }

            // 3. Map Python response to C# response
            var result = MapToCSharpResponse(pythonResponse, request);

            _logger.LogInformation(
                "Successfully generated {Count} questions using {Model}",
                result.Questions.Count,
                pythonResponse.Metadata?.AiModel);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Python AI service");
            return new AiGenerateQuestionsResponse
            {
                Success = false,
                Error = "Failed to connect to AI service",
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating questions");
            return new AiGenerateQuestionsResponse
            {
                Success = false,
                Error = "Unexpected error",
                Message = ex.Message
            };
        }
    }

    public async Task<AiGradeAnswerResponse> GradeAnswerAsync(AiGradeAnswerRequest request)
    {
        // TODO: Implement grading endpoint when Python API supports it
        _logger.LogWarning("GradeAnswerAsync not yet implemented");
        return new AiGradeAnswerResponse
        {
            Success = false,
            Feedback = "Grading not yet implemented"
        };
    }

    /// <summary>
    /// Map C# request to Python API V2 format
    /// </summary>
    private object MapToPythonRequest(AiGenerateQuestionsRequest request)
    {
        // Map C# QuestionType enum to Python string format
        var questionTypes = request.QuestionTypes
            .Select(MapQuestionTypeToString)
            .ToList();

        // Map difficulty
        string? difficulty = request.Difficulty switch
        {
            DifficultyLevel.Easy => "Easy",
            DifficultyLevel.Medium => "Medium",
            DifficultyLevel.Hard => "Hard",
            DifficultyLevel.Expert => "Hard", // Map Expert to Hard for Python
            _ => null
        };

        // Map language
        string language = request.Language switch
        {
            "vi" or "Vietnamese" => "Vietnamese",
            _ => "English"
        };

        // Build skills array if SkillId or SkillName provided
        List<object>? skills = null;
        if (request.SkillId.HasValue || !string.IsNullOrEmpty(request.SkillName))
        {
            skills = new List<object>
            {
                new
                {
                    skill_id = request.SkillId?.ToString(),
                    skill_name = request.SkillName ?? "Unknown Skill",
                    skill_code = request.SkillCode ?? "UNKNOWN"
                }
            };
        }

        // Build target proficiency level array
        List<int>? targetLevels = null;
        if (request.TargetLevel.HasValue)
        {
            targetLevels = new List<int> { (int)request.TargetLevel.Value };
        }

        return new
        {
            question_type = questionTypes,
            language = language,
            number_of_questions = request.QuestionCount,
            skills = skills,
            target_proficiency_level = targetLevels,
            difficulty = difficulty,
            additional_context = request.AdditionalContext
        };
    }

    /// <summary>
    /// Map Python response to C# DTO
    /// </summary>
    private AiGenerateQuestionsResponse MapToCSharpResponse(
        PythonApiResponse pythonResponse,
        AiGenerateQuestionsRequest originalRequest)
    {
        var questions = pythonResponse.Questions
            .Select(q => MapPythonQuestionToCSharp(q, originalRequest))
            .ToList();

        var metadata = pythonResponse.Metadata != null
            ? new AiGenerationMetadata
            {
                Model = pythonResponse.Metadata.AiModel ?? "gemini-2.0-flash-exp",
                GeneratedAt = DateTime.TryParse(pythonResponse.Metadata.GenerationTimestamp, out var dt)
                    ? dt
                    : DateTime.UtcNow,
                TokensUsed = 0, // Not provided by Python API
                GenerationTimeMs = 0 // Not provided by Python API
            }
            : null;

        return new AiGenerateQuestionsResponse
        {
            Success = true,
            Questions = questions,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Map a single Python question to C# DTO
    /// </summary>
    private AiGeneratedQuestion MapPythonQuestionToCSharp(
        PythonQuestion pythonQ,
        AiGenerateQuestionsRequest originalRequest)
    {
        var question = new AiGeneratedQuestion
        {
            Content = pythonQ.Content ?? string.Empty,
            QuestionType = MapStringToQuestionType(pythonQ.Type),
            AssessmentType = originalRequest.AssessmentType,
            Difficulty = MapStringToDifficulty(pythonQ.Difficulty),
            TargetLevel = pythonQ.TargetLevel.HasValue
                ? (ProficiencyLevel)pythonQ.TargetLevel.Value
                : originalRequest.TargetLevel,
            SkillId = pythonQ.SkillId != null && Guid.TryParse(pythonQ.SkillId, out var skillId)
                ? skillId
                : originalRequest.SkillId,
            SuggestedPoints = pythonQ.Points ?? 10,
            SuggestedTimeSeconds = pythonQ.TimeLimitSeconds,
            Tags = pythonQ.Tags ?? new List<string>(),
            Explanation = pythonQ.Explanation,
            CodeSnippet = pythonQ.CodeSnippet,
            GradingRubric = pythonQ.GradingRubric
        };

        // Map options for multiple choice questions
        if (pythonQ.Options != null && pythonQ.Options.Any())
        {
            question.Options = pythonQ.Options
                .OrderBy(o => o.DisplayOrder ?? 0)
                .Select(o => new AiGeneratedOption
                {
                    Content = o.Content ?? string.Empty,
                    IsCorrect = o.IsCorrect ?? false,
                    Explanation = o.Explanation
                })
                .ToList();
        }

        return question;
    }

    /// <summary>
    /// Map C# QuestionType enum to Python string
    /// </summary>
    private string MapQuestionTypeToString(QuestionType type) => type switch
    {
        QuestionType.MultipleChoice => "Multiple Choice",
        QuestionType.MultipleAnswer => "Multiple Answer",
        QuestionType.TrueFalse => "True/False",
        QuestionType.ShortAnswer => "Short Answer",
        QuestionType.LongAnswer => "Long Answer",
        QuestionType.CodingChallenge => "Coding Challenge",
        QuestionType.Scenario => "Scenario",
        QuestionType.SituationalJudgment => "Situational Judgment",
        QuestionType.Rating => "Rating",
        _ => "Multiple Choice"
    };

    /// <summary>
    /// Map Python string to C# QuestionType enum
    /// </summary>
    private QuestionType MapStringToQuestionType(string? type) => type switch
    {
        "MultipleChoice" => QuestionType.MultipleChoice,
        "MultipleAnswer" => QuestionType.MultipleAnswer,
        "TrueFalse" => QuestionType.TrueFalse,
        "ShortAnswer" => QuestionType.ShortAnswer,
        "LongAnswer" => QuestionType.LongAnswer,
        "CodingChallenge" => QuestionType.CodingChallenge,
        "Scenario" => QuestionType.Scenario,
        "SituationalJudgment" => QuestionType.SituationalJudgment,
        "Rating" => QuestionType.Rating,
        _ => QuestionType.MultipleChoice
    };

    /// <summary>
    /// Map Python difficulty string to C# enum
    /// </summary>
    private DifficultyLevel? MapStringToDifficulty(string? difficulty) => difficulty switch
    {
        "Easy" => DifficultyLevel.Easy,
        "Medium" => DifficultyLevel.Medium,
        "Hard" => DifficultyLevel.Hard,
        "Expert" => DifficultyLevel.Expert,
        _ => null
    };
}

#region Python API DTOs

/// <summary>
/// Python API V2 response structure
/// </summary>
internal class PythonApiResponse
{
    [JsonPropertyName("questions")]
    public List<PythonQuestion> Questions { get; set; } = new();

    [JsonPropertyName("metadata")]
    public PythonMetadata? Metadata { get; set; }
}

internal class PythonQuestion
{
    [JsonPropertyName("skill_id")]
    public string? SkillId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("code_snippet")]
    public string? CodeSnippet { get; set; }

    [JsonPropertyName("target_level")]
    public int? TargetLevel { get; set; }

    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; }

    [JsonPropertyName("points")]
    public int? Points { get; set; }

    [JsonPropertyName("time_limit_seconds")]
    public int? TimeLimitSeconds { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("options")]
    public List<PythonOption>? Options { get; set; }

    [JsonPropertyName("grading_rubric")]
    public string? GradingRubric { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("hints")]
    public List<string>? Hints { get; set; }
}

internal class PythonOption
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("is_correct")]
    public bool? IsCorrect { get; set; }

    [JsonPropertyName("display_order")]
    public int? DisplayOrder { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("effectiveness_level")]
    public string? EffectivenessLevel { get; set; }
}

internal class PythonMetadata
{
    [JsonPropertyName("total_questions")]
    public int TotalQuestions { get; set; }

    [JsonPropertyName("generation_timestamp")]
    public string? GenerationTimestamp { get; set; }

    [JsonPropertyName("ai_model")]
    public string? AiModel { get; set; }

    [JsonPropertyName("skill_id")]
    public string? SkillId { get; set; }

    [JsonPropertyName("skill_name")]
    public string? SkillName { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

#endregion
