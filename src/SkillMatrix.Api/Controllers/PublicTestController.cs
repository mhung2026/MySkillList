using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Api.Controllers;

/// <summary>
/// Public endpoints for taking tests via shared links (no authentication required)
/// </summary>
[ApiController]
[Route("api/public/test")]
public class PublicTestController : ControllerBase
{
    private readonly IAssessmentService _service;

    public PublicTestController(IAssessmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get test info by assessment ID (for shared link)
    /// </summary>
    [HttpGet("{assessmentId}")]
    public async Task<ActionResult<StartAssessmentResponse>> GetTestByLink(Guid assessmentId)
    {
        try
        {
            var result = await _service.GetInProgressAssessmentAsync(assessmentId);
            if (result == null)
            {
                // Try to get the assessment details
                var assessment = await _service.GetByIdAsync(assessmentId);
                if (assessment == null)
                    return NotFound(new { error = "Test not found" });

                // If assessment is not started yet, return basic info
                return Ok(new {
                    assessmentId = assessment.Id,
                    title = assessment.Title,
                    status = assessment.Status,
                    needsStart = true
                });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Start test via shared link
    /// </summary>
    [HttpPost("{assessmentId}/start")]
    public async Task<ActionResult<StartAssessmentResponse>> StartTestByLink(Guid assessmentId)
    {
        try
        {
            // Get assessment to find template and employee
            var assessment = await _service.GetByIdAsync(assessmentId);
            if (assessment == null)
                return NotFound(new { error = "Test not found" });

            // If already in progress, return continue
            if (assessment.Status == AssessmentStatus.InProgress)
            {
                var inProgress = await _service.GetInProgressAssessmentAsync(assessmentId);
                return Ok(inProgress);
            }

            // Start the assessment
            var request = new StartAssessmentRequest
            {
                EmployeeId = assessment.EmployeeId,
                TestTemplateId = assessment.TestTemplateId ?? Guid.Empty
            };
            var result = await _service.StartAssessmentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit answer via shared link
    /// </summary>
    [HttpPost("{assessmentId}/answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswerByLink(
        Guid assessmentId,
        [FromBody] PublicSubmitAnswerRequest request)
    {
        try
        {
            var submitRequest = new SubmitAnswerRequest
            {
                AssessmentId = assessmentId,
                QuestionId = request.QuestionId,
                SelectedOptionIds = request.SelectedOptionIds,
                TextResponse = request.TextResponse,
                CodeResponse = request.CodeResponse,
                TimeSpentSeconds = request.TimeSpentSeconds
            };
            var result = await _service.SubmitAnswerAsync(submitRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit test via shared link
    /// </summary>
    [HttpPost("{assessmentId}/submit")]
    public async Task<ActionResult<AssessmentResultDto>> SubmitTestByLink(Guid assessmentId)
    {
        try
        {
            var result = await _service.SubmitAssessmentAsync(assessmentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get result via shared link
    /// </summary>
    [HttpGet("{assessmentId}/result")]
    public async Task<ActionResult<AssessmentResultDto>> GetResultByLink(Guid assessmentId)
    {
        var result = await _service.GetResultAsync(assessmentId);
        if (result == null)
            return NotFound(new { error = "Result not found" });
        return Ok(result);
    }
}

/// <summary>
/// Request for submitting answer via public link
/// </summary>
public class PublicSubmitAnswerRequest
{
    public Guid QuestionId { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; }
    public string? TextResponse { get; set; }
    public string? CodeResponse { get; set; }
    public int? TimeSpentSeconds { get; set; }
}
