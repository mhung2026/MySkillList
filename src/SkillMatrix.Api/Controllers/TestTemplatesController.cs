using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestTemplatesController : ControllerBase
{
    private readonly ITestTemplateService _service;

    public TestTemplatesController(ITestTemplateService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get list of test templates with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TestTemplateListDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, searchTerm, includeInactive);
        return Ok(result);
    }

    /// <summary>
    /// Get test template details by ID (including sections and questions)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TestTemplateDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Create new test template
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TestTemplateDto>> Create([FromBody] CreateTestTemplateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required");

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update test template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TestTemplateDto>> Update(Guid id, [FromBody] UpdateTestTemplateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Delete test template (soft delete)
    /// </summary>
    [HttpPost("{id}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Toggle active status of test template
    /// </summary>
    [HttpPost("{id}/toggle-active")]
    public async Task<ActionResult> ToggleActive(Guid id)
    {
        var success = await _service.ToggleActiveAsync(id);
        if (!success)
            return NotFound();
        return Ok();
    }

    // ==================== SECTION ENDPOINTS ====================

    /// <summary>
    /// Create new section in test template
    /// </summary>
    [HttpPost("sections")]
    public async Task<ActionResult<TestSectionDto>> CreateSection([FromBody] CreateTestSectionDto dto)
    {
        if (dto.TestTemplateId == Guid.Empty)
            return BadRequest("TestTemplateId is required");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required");

        var result = await _service.CreateSectionAsync(dto);
        return Created($"/api/testtemplates/{dto.TestTemplateId}", result);
    }

    /// <summary>
    /// Update section
    /// </summary>
    [HttpPut("sections/{id}")]
    public async Task<ActionResult<TestSectionDto>> UpdateSection(Guid id, [FromBody] UpdateTestSectionDto dto)
    {
        var result = await _service.UpdateSectionAsync(id, dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Delete section
    /// </summary>
    [HttpPost("sections/{id}/delete")]
    public async Task<ActionResult> DeleteSection(Guid id)
    {
        var success = await _service.DeleteSectionAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}
