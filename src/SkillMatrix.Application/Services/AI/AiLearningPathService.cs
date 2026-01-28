using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillMatrix.Infrastructure.Repositories;

namespace SkillMatrix.Application.Services.AI;

public interface IAiLearningPathService
{
    Task<AiLearningPathResponse> GenerateLearningPathAsync(AiLearningPathRequest request);
}

public class AiLearningPathService : IAiLearningPathService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ICourseraCourseRepository _courseraRepo;
    private readonly ILogger<AiLearningPathService> _logger;

    public AiLearningPathService(
        HttpClient httpClient,
        IConfiguration configuration,
        ICourseraCourseRepository courseraRepo,
        ILogger<AiLearningPathService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _courseraRepo = courseraRepo;
        _logger = logger;
    }

    public async Task<AiLearningPathResponse> GenerateLearningPathAsync(AiLearningPathRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Generating AI learning path for skill {SkillName} (Code: {SkillCode}) from level {From} to {To}",
                request.SkillName, request.SkillCode, request.CurrentLevel, request.TargetLevel);

            // Get Coursera courses from database
            var courses = await GetCourseraCoursesAsync(request.SkillCode, request.SkillName);

            _logger.LogInformation(
                "Found {Count} Coursera courses for skill {SkillCode}",
                courses.Count, request.SkillCode);

            // Transform to AI service format
            var aiResources = courses.Select(c => new
            {
                id = c.Id.ToString(),
                title = c.Title,
                type = "Course",
                description = c.Description,
                estimated_hours = ParseDuration(c.Duration),
                difficulty = MapLevel(c.Level),
                from_level = request.CurrentLevel,
                to_level = request.TargetLevel
            }).ToList();

            // Call AI service
            var baseUrl = _configuration["AiService:BaseUrl"];
            var endpoint = $"{baseUrl}/api/v2/generate-learning-path";

            var aiRequest = new
            {
                employee_id = request.EmployeeId.ToString(),
                employee_name = request.EmployeeName,
                skill_id = request.SkillId?.ToString(),
                skill_name = request.SkillName,
                skill_code = request.SkillCode,
                current_level = request.CurrentLevel,
                target_level = request.TargetLevel,
                skill_description = request.SkillDescription,
                available_resources = aiResources,
                time_constraint_months = request.TimeConstraintMonths,
                language = request.Language ?? "vi"
            };

            var requestJson = JsonSerializer.Serialize(aiRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogDebug("Sending request to {Endpoint}: {Request}", endpoint, requestJson);

            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "AI service returned error {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);
            }

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var aiResponse = JsonSerializer.Deserialize<AiLearningPathApiResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (aiResponse == null)
            {
                throw new Exception("Failed to deserialize AI service response");
            }

            _logger.LogInformation(
                "Successfully generated learning path with {Items} items",
                aiResponse.LearningItems?.Count ?? 0);

            // Map AI response to domain model
            var result = new AiLearningPathResponse
            {
                Success = aiResponse.Success,
                PathTitle = aiResponse.PathTitle ?? $"Lộ trình phát triển {request.SkillName}",
                PathDescription = aiResponse.PathDescription ?? "",
                EstimatedTotalHours = aiResponse.EstimatedTotalHours ?? 0,
                EstimatedDurationWeeks = aiResponse.EstimatedDurationWeeks ?? 0,
                AiRationale = aiResponse.AiRationale ?? "",
                KeySuccessFactors = aiResponse.KeySuccessFactors ?? new List<string>(),
                PotentialChallenges = aiResponse.PotentialChallenges ?? new List<string>(),
                LearningItems = aiResponse.LearningItems?.Select(item => new AiLearningItem
                {
                    Order = item.Order,
                    Title = item.Title ?? "",
                    Description = item.Description ?? "",
                    ItemType = item.ItemType ?? "Course",
                    EstimatedHours = item.EstimatedHours ?? 0,
                    TargetLevelAfter = item.TargetLevelAfter ?? request.TargetLevel,
                    SuccessCriteria = item.SuccessCriteria ?? "",
                    ResourceId = item.ResourceId,
                    CourseUrl = GetCourseUrl(item.ResourceId, courses)
                }).ToList() ?? new List<AiLearningItem>(),
                Milestones = aiResponse.Milestones?.Select(m => new AiLearningMilestone
                {
                    AfterItem = m.AfterItem,
                    Description = m.Description ?? "",
                    ExpectedLevel = m.ExpectedLevel
                }).ToList() ?? new List<AiLearningMilestone>()
            };

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling AI service for learning path generation");
            throw new Exception($"Failed to connect to AI service: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating learning path for skill {SkillName}",
                request.SkillName);
            throw;
        }
    }

    private async Task<List<CourseraCourseModel>> GetCourseraCoursesAsync(string? skillCode, string skillName)
    {
        List<CourseraCourseModel> courses;

        if (!string.IsNullOrEmpty(skillCode))
        {
            courses = await _courseraRepo.GetCoursesBySkillCodeAsync(skillCode);
        }
        else
        {
            courses = await _courseraRepo.SearchCoursesBySkillNameAsync(skillName);
        }

        return courses;
    }

    private string? GetCourseUrl(string? resourceId, List<CourseraCourseModel> courses)
    {
        if (string.IsNullOrEmpty(resourceId))
            return null;

        if (int.TryParse(resourceId, out var courseId))
        {
            return courses.FirstOrDefault(c => c.Id == courseId)?.Url;
        }

        return null;
    }

    private int? ParseDuration(string? duration)
    {
        if (string.IsNullOrEmpty(duration)) return null;

        // Parse duration like "Approx. 20 hours" or "3 months"
        var match = Regex.Match(duration, @"(\d+)\s*(hour|month|week)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var value = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value.ToLower();

            return unit switch
            {
                "hour" => value,
                "week" => value * 10, // Assume 10 hours/week
                "month" => value * 40, // Assume 40 hours/month
                _ => null
            };
        }

        return null;
    }

    private string? MapLevel(string? level)
    {
        if (string.IsNullOrEmpty(level)) return null;

        var lowerLevel = level.ToLower();
        if (lowerLevel.Contains("beginner")) return "Easy";
        if (lowerLevel.Contains("intermediate")) return "Medium";
        if (lowerLevel.Contains("advanced")) return "Hard";

        return "Medium";
    }
}

#region DTOs

public class AiLearningPathRequest
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? SkillCode { get; set; }
    public string? SkillDescription { get; set; }
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
    public int? TimeConstraintMonths { get; set; }
    public string? Language { get; set; } = "vi";
}

public class AiLearningPathResponse
{
    public bool Success { get; set; }
    public string PathTitle { get; set; } = string.Empty;
    public string PathDescription { get; set; } = string.Empty;
    public int EstimatedTotalHours { get; set; }
    public int EstimatedDurationWeeks { get; set; }
    public List<AiLearningItem> LearningItems { get; set; } = new();
    public List<AiLearningMilestone> Milestones { get; set; } = new();
    public string AiRationale { get; set; } = string.Empty;
    public List<string> KeySuccessFactors { get; set; } = new();
    public List<string> PotentialChallenges { get; set; } = new();
}

public class AiLearningItem
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public int TargetLevelAfter { get; set; }
    public string SuccessCriteria { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? CourseUrl { get; set; }
}

public class AiLearningMilestone
{
    public int AfterItem { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ExpectedLevel { get; set; }
}

// Internal API response models from AI service
internal class AiLearningPathApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("path_title")]
    public string? PathTitle { get; set; }

    [JsonPropertyName("path_description")]
    public string? PathDescription { get; set; }

    [JsonPropertyName("estimated_total_hours")]
    public int? EstimatedTotalHours { get; set; }

    [JsonPropertyName("estimated_duration_weeks")]
    public int? EstimatedDurationWeeks { get; set; }

    [JsonPropertyName("learning_items")]
    public List<AiLearningItemApi>? LearningItems { get; set; }

    [JsonPropertyName("milestones")]
    public List<AiLearningMilestoneApi>? Milestones { get; set; }

    [JsonPropertyName("ai_rationale")]
    public string? AiRationale { get; set; }

    [JsonPropertyName("key_success_factors")]
    public List<string>? KeySuccessFactors { get; set; }

    [JsonPropertyName("potential_challenges")]
    public List<string>? PotentialChallenges { get; set; }
}

internal class AiLearningItemApi
{
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("item_type")]
    public string? ItemType { get; set; }

    [JsonPropertyName("estimated_hours")]
    public int? EstimatedHours { get; set; }

    [JsonPropertyName("target_level_after")]
    public int? TargetLevelAfter { get; set; }

    [JsonPropertyName("success_criteria")]
    public string? SuccessCriteria { get; set; }

    [JsonPropertyName("resource_id")]
    public string? ResourceId { get; set; }
}

internal class AiLearningMilestoneApi
{
    [JsonPropertyName("after_item")]
    public int AfterItem { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("expected_level")]
    public int ExpectedLevel { get; set; }
}

#endregion
