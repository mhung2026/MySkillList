using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.Services.AI;

/// <summary>
/// AI service for evaluating assessment results and determining skill levels
/// </summary>
public interface IAiAssessmentEvaluatorService
{
    Task<AiEvaluateAssessmentResponse> EvaluateAssessmentAsync(AiEvaluateAssessmentRequest request);
}

public class PythonAiAssessmentEvaluatorService : IAiAssessmentEvaluatorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonAiAssessmentEvaluatorService> _logger;
    private readonly AiServiceOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public PythonAiAssessmentEvaluatorService(
        HttpClient httpClient,
        ILogger<PythonAiAssessmentEvaluatorService> logger,
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

    public async Task<AiEvaluateAssessmentResponse> EvaluateAssessmentAsync(AiEvaluateAssessmentRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Evaluating assessment for employee {EmployeeId} with {SkillCount} skills",
                request.EmployeeId,
                request.Assessments.Count);

            // Map C# request to Python API format
            var pythonRequest = MapToPythonRequest(request);

            // Call Python API
            var endpoint = $"{_options.BaseUrl}/api/v2/evaluate-assessments";
            _logger.LogDebug("Calling Python AI API: {Endpoint}", endpoint);

            var requestJson = JsonSerializer.Serialize(pythonRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogDebug("Request: {Request}", requestJson);

            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Python AI API returned error {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);

                return new AiEvaluateAssessmentResponse
                {
                    Success = false,
                    Results = new List<AiSkillEvaluationResult>()
                };
            }

            // Read and parse response
            var rawResponse = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Python AI raw response: {Response}", rawResponse);

            var pythonResponse = JsonSerializer.Deserialize<PythonEvaluateAssessmentResponse>(rawResponse, _jsonOptions);

            if (pythonResponse == null)
            {
                _logger.LogWarning("Python AI API returned null response");
                return new AiEvaluateAssessmentResponse
                {
                    Success = false,
                    Results = new List<AiSkillEvaluationResult>()
                };
            }

            // Map Python response to C# response
            var result = MapToCSharpResponse(pythonResponse);

            _logger.LogInformation(
                "Successfully evaluated {ResultCount} skills",
                result.Results.Count);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Python AI assessment evaluator service");
            return new AiEvaluateAssessmentResponse
            {
                Success = false,
                Results = new List<AiSkillEvaluationResult>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error evaluating assessment");
            return new AiEvaluateAssessmentResponse
            {
                Success = false,
                Results = new List<AiSkillEvaluationResult>()
            };
        }
    }

    private object MapToPythonRequest(AiEvaluateAssessmentRequest request)
    {
        return new
        {
            assessments = request.Assessments.Select(a => new
            {
                skill_id = a.SkillId?.ToString(),
                skill_name = a.SkillName,
                responses = a.Responses.Select(r => new
                {
                    question_id = r.QuestionId?.ToString(),
                    question_type = r.QuestionType,
                    target_level = r.TargetLevel,
                    is_correct = r.IsCorrect,
                    score = r.Score,
                    max_score = r.MaxScore
                }).ToList()
            }).ToList()
        };
    }

    private AiEvaluateAssessmentResponse MapToCSharpResponse(PythonEvaluateAssessmentResponse pythonResponse)
    {
        return new AiEvaluateAssessmentResponse
        {
            Success = true,
            Results = pythonResponse.Results?.Select(r => new AiSkillEvaluationResult
            {
                SkillId = Guid.TryParse(r.SkillId, out var skillId) ? skillId : (Guid?)null,
                SkillName = r.SkillName ?? "",
                CurrentLevel = r.CurrentLevel,
                TotalQuestions = r.TotalQuestions,
                OverallScorePercentage = r.OverallScorePercentage
            }).ToList() ?? new List<AiSkillEvaluationResult>(),
            Summary = pythonResponse.Summary != null ? new AiEvaluationSummary
            {
                TotalSkills = pythonResponse.Summary.TotalSkills,
                SkillsEvaluated = pythonResponse.Summary.SkillsEvaluated,
                AverageLevel = pythonResponse.Summary.AverageLevel
            } : null
        };
    }
}

#region DTOs

/// <summary>
/// Request to evaluate assessment and determine skill levels
/// </summary>
public class AiEvaluateAssessmentRequest
{
    public Guid EmployeeId { get; set; }
    public List<AiSkillAssessment> Assessments { get; set; } = new();
}

public class AiSkillAssessment
{
    public Guid? SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public List<AiQuestionResponse> Responses { get; set; } = new();
}

public class AiQuestionResponse
{
    public Guid? QuestionId { get; set; }
    public string QuestionType { get; set; } = string.Empty; // "MultipleChoice", "CodingChallenge", etc.
    public int TargetLevel { get; set; }
    public bool? IsCorrect { get; set; }
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
}

/// <summary>
/// Response from AI assessment evaluation
/// </summary>
public class AiEvaluateAssessmentResponse
{
    public bool Success { get; set; }
    public List<AiSkillEvaluationResult> Results { get; set; } = new();
    public AiEvaluationSummary? Summary { get; set; }
}

public class AiSkillEvaluationResult
{
    public Guid? SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int CurrentLevel { get; set; } // AI-determined SFIA level (1-7)
    public int TotalQuestions { get; set; }
    public double OverallScorePercentage { get; set; }
}

public class AiEvaluationSummary
{
    public int TotalSkills { get; set; }
    public int SkillsEvaluated { get; set; }
    public double AverageLevel { get; set; }
}

#endregion

#region Python API DTOs

internal class PythonEvaluateAssessmentResponse
{
    [JsonPropertyName("results")]
    public List<PythonSkillEvaluationResult>? Results { get; set; }

    [JsonPropertyName("summary")]
    public PythonEvaluationSummary? Summary { get; set; }
}

internal class PythonSkillEvaluationResult
{
    [JsonPropertyName("skill_id")]
    public string? SkillId { get; set; }

    [JsonPropertyName("skill_name")]
    public string? SkillName { get; set; }

    [JsonPropertyName("current_level")]
    public int CurrentLevel { get; set; }

    [JsonPropertyName("min_defined_level")]
    public int? MinDefinedLevel { get; set; }

    [JsonPropertyName("consecutive_levels_passed")]
    public int? ConsecutiveLevelsPassed { get; set; }

    [JsonPropertyName("highest_level_with_responses")]
    public int? HighestLevelWithResponses { get; set; }

    [JsonPropertyName("total_questions")]
    public int TotalQuestions { get; set; }

    [JsonPropertyName("overall_score_percentage")]
    public double OverallScorePercentage { get; set; }
}

internal class PythonEvaluationSummary
{
    [JsonPropertyName("total_skills")]
    public int TotalSkills { get; set; }

    [JsonPropertyName("skills_evaluated")]
    public int SkillsEvaluated { get; set; }

    [JsonPropertyName("average_level")]
    public double AverageLevel { get; set; }
}

#endregion
