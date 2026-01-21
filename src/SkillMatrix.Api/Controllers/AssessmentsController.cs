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
    /// Lấy danh sách tests có thể làm
    /// </summary>
    [HttpGet("available/{employeeId}")]
    public async Task<ActionResult<List<AvailableTestDto>>> GetAvailableTests(Guid employeeId)
    {
        var result = await _service.GetAvailableTestsAsync(employeeId);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách assessments của employee
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
    /// Lấy chi tiết assessment
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
    /// Bắt đầu làm bài test
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
    /// Lấy bài test đang làm dở
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
    /// Submit câu trả lời cho 1 câu hỏi
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
    /// Nộp bài test (hoàn thành)
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
    /// Xem kết quả bài test
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
