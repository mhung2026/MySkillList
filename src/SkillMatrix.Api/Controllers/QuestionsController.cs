using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _service;

    public QuestionsController(IQuestionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách câu hỏi với filter và phân trang
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<QuestionListDto>>> GetAll([FromQuery] QuestionFilterRequest filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết câu hỏi theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Tạo câu hỏi mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create([FromBody] CreateQuestionDto dto)
    {
        if (dto.SkillId == Guid.Empty)
            return BadRequest("SkillId is required");

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Content is required");

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cập nhật câu hỏi
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionDto>> Update(Guid id, [FromBody] UpdateQuestionDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Xóa câu hỏi (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Toggle trạng thái active của câu hỏi
    /// </summary>
    [HttpPost("{id}/toggle-active")]
    public async Task<ActionResult> ToggleActive(Guid id)
    {
        var success = await _service.ToggleActiveAsync(id);
        if (!success)
            return NotFound();
        return Ok();
    }

    /// <summary>
    /// Tạo nhiều câu hỏi cùng lúc (bulk create)
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<List<QuestionDto>>> CreateBulk([FromBody] List<CreateQuestionDto> dtos)
    {
        if (!dtos.Any())
            return BadRequest("At least one question is required");

        var results = await _service.CreateBulkAsync(dtos);
        return Ok(results);
    }

    /// <summary>
    /// Generate câu hỏi từ AI và thêm vào section
    /// </summary>
    [HttpPost("generate-ai")]
    public async Task<ActionResult<List<QuestionDto>>> GenerateFromAi([FromBody] GenerateAiQuestionsRequest request)
    {
        if (request.SectionId == Guid.Empty)
            return BadRequest("SectionId is required");

        var aiRequest = new AiGenerateQuestionsRequest
        {
            SkillId = request.SkillId,
            SkillName = request.SkillName,
            TargetLevel = request.TargetLevel,
            QuestionCount = request.QuestionCount,
            AssessmentType = request.AssessmentType,
            Difficulty = request.Difficulty,
            Language = request.Language,
            AdditionalContext = request.AdditionalContext,
            JobRole = request.JobRole,
            SectionId = request.SectionId
        };

        var results = await _service.CreateFromAiAsync(request.SectionId, aiRequest);

        if (!results.Any())
            return BadRequest("Failed to generate questions from AI");

        return Ok(results);
    }
}

/// <summary>
/// Request để generate câu hỏi từ AI và thêm vào section
/// </summary>
public class GenerateAiQuestionsRequest : AiGenerateQuestionsRequest
{
    /// <summary>
    /// Section ID để add câu hỏi vào (bắt buộc)
    /// </summary>
    public new Guid SectionId { get; set; }
}
