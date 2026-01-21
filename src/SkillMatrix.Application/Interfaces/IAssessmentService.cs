using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;

namespace SkillMatrix.Application.Interfaces;

public interface IAssessmentService
{
    /// <summary>
    /// Lấy danh sách assessments của employee
    /// </summary>
    Task<PagedResult<AssessmentListDto>> GetByEmployeeAsync(Guid employeeId, int pageNumber, int pageSize);

    /// <summary>
    /// Lấy chi tiết assessment
    /// </summary>
    Task<AssessmentDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Bắt đầu làm bài test - tạo assessment mới
    /// </summary>
    Task<StartAssessmentResponse> StartAssessmentAsync(StartAssessmentRequest request);

    /// <summary>
    /// Lấy bài test đang làm dở
    /// </summary>
    Task<StartAssessmentResponse?> GetInProgressAssessmentAsync(Guid assessmentId);

    /// <summary>
    /// Submit câu trả lời cho 1 câu hỏi
    /// </summary>
    Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request);

    /// <summary>
    /// Nộp bài test (hoàn thành)
    /// </summary>
    Task<AssessmentResultDto> SubmitAssessmentAsync(Guid assessmentId);

    /// <summary>
    /// Xem kết quả bài test
    /// </summary>
    Task<AssessmentResultDto?> GetResultAsync(Guid assessmentId);

    /// <summary>
    /// Lấy danh sách test templates có thể làm
    /// </summary>
    Task<List<AvailableTestDto>> GetAvailableTestsAsync(Guid employeeId);
}

/// <summary>
/// Test có thể làm
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
