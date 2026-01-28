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
/// AI service implementation calling Python API for skill gap analysis
/// </summary>
public class PythonAiSkillAnalyzerService : IAiSkillAnalyzerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonAiSkillAnalyzerService> _logger;
    private readonly AiServiceOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public PythonAiSkillAnalyzerService(
        HttpClient httpClient,
        ILogger<PythonAiSkillAnalyzerService> logger,
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

    public async Task<AiAnalyzeSkillGapResponse> AnalyzeSkillGapsAsync(AiAnalyzeSkillGapRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Analyzing skill gaps for employee {EmployeeId} with {Count} skills",
                request.EmployeeId,
                request.CurrentSkills.Count);

            // 1. Map C# request to Python API V2 request
            var pythonRequest = MapToPythonRequest(request);

            // 2. Call Python API
            var endpoint = $"{_options.BaseUrl}/api/v2/analyze-gaps";
            _logger.LogDebug("Calling Python AI API: {Endpoint}", endpoint);

            var response = await _httpClient.PostAsJsonAsync(endpoint, pythonRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Python AI API returned error {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);

                return new AiAnalyzeSkillGapResponse
                {
                    Success = false,
                    Gaps = new List<AiSkillGapAnalysis>(),
                    Recommendations = new List<AiLearningRecommendation>(),
                    OverallAssessment = $"AI service error: {response.StatusCode}"
                };
            }

            // Read raw response for debugging
            var rawResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Python AI raw response: {Response}", rawResponse);

            var pythonResponse = JsonSerializer.Deserialize<PythonAnalyzeGapsResponse>(rawResponse, _jsonOptions);

            if (pythonResponse == null)
            {
                _logger.LogWarning("Python AI API returned null response");
                return new AiAnalyzeSkillGapResponse
                {
                    Success = false,
                    Gaps = new List<AiSkillGapAnalysis>(),
                    Recommendations = new List<AiLearningRecommendation>(),
                    OverallAssessment = "No response from AI service"
                };
            }

            // 3. Map Python response to C# response
            var result = MapToCSharpResponse(pythonResponse);

            _logger.LogInformation(
                "Successfully analyzed {GapCount} skill gaps with {RecCount} recommendations",
                result.Gaps.Count,
                result.Recommendations?.Count ?? 0);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Python AI service");
            return new AiAnalyzeSkillGapResponse
            {
                Success = false,
                Gaps = new List<AiSkillGapAnalysis>(),
                Recommendations = new List<AiLearningRecommendation>(),
                OverallAssessment = "Failed to connect to AI service"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error analyzing skill gaps");
            return new AiAnalyzeSkillGapResponse
            {
                Success = false,
                Gaps = new List<AiSkillGapAnalysis>(),
                Recommendations = new List<AiLearningRecommendation>(),
                OverallAssessment = $"Unexpected error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Map C# request to Python API V2 format
    /// </summary>
    private object MapToPythonRequest(AiAnalyzeSkillGapRequest request)
    {
        var gaps = request.CurrentSkills.Select(s => new
        {
            skill_id = s.SkillId.ToString(),
            skill_name = s.SkillName,
            skill_code = s.SkillCode,
            current_level = (int)s.CurrentLevel,
            required_level = s.RequiredLevel.HasValue ? (int)s.RequiredLevel.Value : (int?)null,
            skill_description = (string?)null  // Can add if available
        }).ToList();

        return new
        {
            employee_id = request.EmployeeId.ToString(),
            employee_name = request.EmployeeName ?? "Unknown",
            job_role = request.JobRoleName ?? "Unknown Role",
            gaps = gaps,
            language = "vn"
        };
    }

    /// <summary>
    /// Map Python response to C# DTO
    /// </summary>
    private AiAnalyzeSkillGapResponse MapToCSharpResponse(PythonAnalyzeGapsResponse pythonResponse)
    {
        var gaps = pythonResponse.GapAnalyses?
            .Select(MapPythonGapToCSharp)
            .ToList() ?? new List<AiSkillGapAnalysis>();

        // Note: Coursera recommendations will be fetched separately via generate-learning-path API
        var recommendations = new List<AiLearningRecommendation>();

        return new AiAnalyzeSkillGapResponse
        {
            Success = pythonResponse.Success,
            Gaps = gaps,
            Recommendations = recommendations,  // Empty for now, will be populated by learning path API
            OverallAssessment = pythonResponse.OverallSummary ?? "Analysis completed"
        };
    }

    /// <summary>
    /// Map a single Python gap to C# DTO
    /// </summary>
    private AiSkillGapAnalysis MapPythonGapToCSharp(PythonSkillGap pythonGap)
    {
        // Determine priority from gap_size if not provided
        var priority = "Medium";
        if (pythonGap.GapSize.HasValue)
        {
            priority = pythonGap.GapSize.Value switch
            {
                >= 3 => "Critical",
                2 => "High",
                1 => "Medium",
                _ => "Low"
            };
        }

        return new AiSkillGapAnalysis
        {
            SkillId = Guid.TryParse(pythonGap.SkillId, out var skillId) ? skillId : Guid.Empty,
            SkillName = pythonGap.SkillName ?? "Unknown",
            CurrentLevel = (ProficiencyLevel)(pythonGap.CurrentLevel ?? 0),
            RequiredLevel = pythonGap.RequiredLevel.HasValue
                ? (ProficiencyLevel)pythonGap.RequiredLevel.Value
                : ProficiencyLevel.Apply,
            GapSize = pythonGap.GapSize ?? 0,
            Priority = priority,
            Analysis = pythonGap.AiAnalysis ?? "",
            Recommendation = pythonGap.AiRecommendation ?? ""
        };
    }

    /// <summary>
    /// Map a single Python recommendation to C# DTO
    /// </summary>
    private AiLearningRecommendation MapPythonRecommendationToCSharp(PythonLearningRecommendation pythonRec)
    {
        return new AiLearningRecommendation
        {
            SkillId = Guid.TryParse(pythonRec.SkillId, out var skillId) ? skillId : Guid.Empty,
            SkillName = pythonRec.SkillName ?? "Unknown",
            RecommendationType = pythonRec.Type ?? "Course",
            Title = pythonRec.Title ?? "",
            Description = pythonRec.Description,
            Url = pythonRec.Url,
            EstimatedHours = pythonRec.EstimatedHours,
            Rationale = pythonRec.Rationale ?? ""
        };
    }
}

#region Python API DTOs

/// <summary>
/// Python API V2 analyze-gaps response structure
/// </summary>
internal class PythonAnalyzeGapsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("gap_analyses")]
    public List<PythonSkillGap>? GapAnalyses { get; set; }

    [JsonPropertyName("overall_summary")]
    public string? OverallSummary { get; set; }

    [JsonPropertyName("priority_order")]
    public List<string>? PriorityOrder { get; set; }

    [JsonPropertyName("recommended_focus_areas")]
    public List<string>? RecommendedFocusAreas { get; set; }
}

internal class PythonSkillGap
{
    [JsonPropertyName("skill_id")]
    public string? SkillId { get; set; }

    [JsonPropertyName("skill_name")]
    public string? SkillName { get; set; }

    [JsonPropertyName("current_level")]
    public int? CurrentLevel { get; set; }

    [JsonPropertyName("required_level")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("gap_size")]
    public int? GapSize { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("ai_analysis")]
    public string? AiAnalysis { get; set; }

    [JsonPropertyName("ai_recommendation")]
    public string? AiRecommendation { get; set; }

    [JsonPropertyName("priority_rationale")]
    public string? PriorityRationale { get; set; }

    [JsonPropertyName("estimated_effort")]
    public string? EstimatedEffort { get; set; }

    [JsonPropertyName("key_actions")]
    public List<string>? KeyActions { get; set; }

    [JsonPropertyName("potential_blockers")]
    public List<string>? PotentialBlockers { get; set; }
}

internal class PythonLearningRecommendation
{
    [JsonPropertyName("skill_id")]
    public string? SkillId { get; set; }

    [JsonPropertyName("skill_name")]
    public string? SkillName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("estimated_hours")]
    public int? EstimatedHours { get; set; }

    [JsonPropertyName("rationale")]
    public string? Rationale { get; set; }
}

#endregion
