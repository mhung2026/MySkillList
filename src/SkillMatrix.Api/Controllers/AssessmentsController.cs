using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _service;

    public AssessmentsController(IAssessmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get list of available tests
    /// </summary>
    [HttpGet("available/{employeeId}")]
    public async Task<ActionResult<List<AvailableTestDto>>> GetAvailableTests(Guid employeeId)
    {
        var result = await _service.GetAvailableTestsAsync(employeeId);
        return Ok(result);
    }

    /// <summary>
    /// Get list of assessments for an employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<PagedResult<AssessmentListDto>>> GetByEmployee(
        Guid employeeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByEmployeeAsync(employeeId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get assessment details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AssessmentDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Start taking a test
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<StartAssessmentResponse>> StartAssessment([FromBody] StartAssessmentRequest request)
    {
        try
        {
            var result = await _service.StartAssessmentAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get in-progress test
    /// </summary>
    [HttpGet("{id}/continue")]
    public async Task<ActionResult<StartAssessmentResponse>> ContinueAssessment(Guid id)
    {
        var result = await _service.GetInProgressAssessmentAsync(id);
        if (result == null)
            return NotFound(new { error = "Assessment not found or already completed" });
        return Ok(result);
    }

    /// <summary>
    /// Submit answer for a question
    /// </summary>
    [HttpPost("answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        try
        {
            var result = await _service.SubmitAnswerAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit test (complete)
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<AssessmentResultDto>> SubmitAssessment(Guid id)
    {
        try
        {
            var result = await _service.SubmitAssessmentAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// View test result
    /// </summary>
    [HttpGet("{id}/result")]
    public async Task<ActionResult<AssessmentResultDto>> GetResult(Guid id)
    {
        var result = await _service.GetResultAsync(id);
        if (result == null)
            return NotFound(new { error = "Result not found or assessment not completed" });
        return Ok(result);
    }
}
