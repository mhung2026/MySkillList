using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _service;

    public SkillsController(ISkillService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all skills with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SkillListDto>>>> GetAll(
        [FromQuery] SkillFilterRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<SkillListDto>>.Ok(result));
    }

    /// <summary>
    /// Get skill by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SkillDto>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<SkillDto>.Fail("Skill not found"));

        return Ok(ApiResponse<SkillDto>.Ok(result));
    }

    /// <summary>
    /// Create new skill
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SkillDto>>> Create(
        [FromBody] CreateSkillDto dto)
    {
        // Check code uniqueness
        if (await _service.CodeExistsAsync(dto.Code))
            return BadRequest(ApiResponse<SkillDto>.Fail($"Code '{dto.Code}' already exists"));

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SkillDto>.Ok(result, "Skill created successfully"));
    }

    /// <summary>
    /// Update skill
    /// </summary>
    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<ApiResponse<SkillDto>>> Update(
        Guid id, [FromBody] UpdateSkillDto dto)
    {
        // Check code uniqueness
        if (await _service.CodeExistsAsync(dto.Code, id))
            return BadRequest(ApiResponse<SkillDto>.Fail($"Code '{dto.Code}' already exists"));

        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<SkillDto>.Ok(result, "Skill updated successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SkillDto>.Fail("Skill not found"));
        }
    }

    /// <summary>
    /// Soft delete skill
    /// </summary>
    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Skill not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Skill deleted successfully"));
    }

    /// <summary>
    /// Toggle active status
    /// </summary>
    [HttpPost("{id:guid}/toggle-active")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid id)
    {
        var result = await _service.ToggleActiveAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Skill not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Status toggled successfully"));
    }

    /// <summary>
    /// Get dropdown list
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<List<SkillDropdownDto>>>> GetDropdown(
        [FromQuery] Guid? subcategoryId = null)
    {
        var result = await _service.GetDropdownAsync(subcategoryId);
        return Ok(ApiResponse<List<SkillDropdownDto>>.Ok(result));
    }

    /// <summary>
    /// Create level definition for a skill
    /// </summary>
    [HttpPost("{skillId:guid}/level-definitions")]
    public async Task<ActionResult<ApiResponse<SkillLevelDefinitionDto>>> CreateLevelDefinition(
        Guid skillId, [FromBody] CreateSkillLevelDefinitionDto dto)
    {
        dto.SkillId = skillId;
        var result = await _service.CreateLevelDefinitionAsync(dto);
        return Ok(ApiResponse<SkillLevelDefinitionDto>.Ok(result, "Level definition created successfully"));
    }

    /// <summary>
    /// Update level definition
    /// </summary>
    [HttpPost("level-definitions/{id:guid}/update")]
    public async Task<ActionResult<ApiResponse<SkillLevelDefinitionDto>>> UpdateLevelDefinition(
        Guid id, [FromBody] CreateSkillLevelDefinitionDto dto)
    {
        try
        {
            var result = await _service.UpdateLevelDefinitionAsync(id, dto);
            return Ok(ApiResponse<SkillLevelDefinitionDto>.Ok(result, "Level definition updated successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SkillLevelDefinitionDto>.Fail("Level definition not found"));
        }
    }

    /// <summary>
    /// Delete level definition
    /// </summary>
    [HttpPost("level-definitions/{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLevelDefinition(Guid id)
    {
        var result = await _service.DeleteLevelDefinitionAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Level definition not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Level definition deleted successfully"));
    }
}
