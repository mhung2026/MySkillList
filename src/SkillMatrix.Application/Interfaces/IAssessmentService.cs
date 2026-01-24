using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;

namespace SkillMatrix.Application.Interfaces;

public interface IAssessmentService
{
    /// <summary>
    /// Get list of assessments for an employee
    /// </summary>
    Task<PagedResult<AssessmentListDto>> GetByEmployeeAsync(Guid employeeId, int pageNumber, int pageSize);

    /// <summary>
    /// Get assessment details
    /// </summary>
    Task<AssessmentDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Start taking a test - create new assessment
    /// </summary>
    Task<StartAssessmentResponse> StartAssessmentAsync(StartAssessmentRequest request);

    /// <summary>
    /// Get in-progress test
    /// </summary>
    Task<StartAssessmentResponse?> GetInProgressAssessmentAsync(Guid assessmentId);

    /// <summary>
    /// Start an existing assessment (for shared links)
    /// </summary>
    Task<StartAssessmentResponse?> StartExistingAssessmentAsync(Guid assessmentId);

    /// <summary>
    /// Submit answer for a question
    /// </summary>
    Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request);

    /// <summary>
    /// Submit test (complete)
    /// </summary>
    Task<AssessmentResultDto> SubmitAssessmentAsync(Guid assessmentId);

    /// <summary>
    /// View test result
    /// </summary>
    Task<AssessmentResultDto?> GetResultAsync(Guid assessmentId);

    /// <summary>
    /// Get list of available test templates
    /// </summary>
    Task<List<AvailableTestDto>> GetAvailableTestsAsync(Guid employeeId);
}

/// <summary>
/// Available test
/// </summary>
public class AvailableTestDto
{
    public Guid TestTemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? TargetSkillName { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int QuestionCount { get; set; }
    public int PassingScore { get; set; }
    public bool HasAttempted { get; set; }
    public int AttemptCount { get; set; }
    public double? BestScore { get; set; }
}
