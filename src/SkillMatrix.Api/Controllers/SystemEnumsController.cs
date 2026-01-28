using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Configuration;
using SkillMatrix.Application.Services;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemEnumsController : ControllerBase
{
    private readonly SystemEnumService _enumService;

    public SystemEnumsController(SystemEnumService enumService)
    {
        _enumService = enumService;
    }

    /// <summary>
    /// Get all enum types with value counts
    /// </summary>
    [HttpGet("types")]
    public async Task<ActionResult<List<EnumTypeDto>>> GetAllTypes()
    {
        var result = await _enumService.GetAllEnumTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get all values for a specific enum type
    /// </summary>
    [HttpGet("values/{enumType}")]
    public async Task<ActionResult<List<SystemEnumValueDto>>> GetValuesByType(
        string enumType,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _enumService.GetValuesByTypeAsync(enumType, includeInactive);
        return Ok(result);
    }

    /// <summary>
    /// Get enum values for dropdown (simplified format)
    /// </summary>
    [HttpGet("dropdown/{enumType}")]
    public async Task<ActionResult<List<EnumDropdownItemDto>>> GetDropdown(
        string enumType,
        [FromQuery] string language = "en")
    {
        var result = await _enumService.GetDropdownAsync(enumType);
        return Ok(result);
    }

    /// <summary>
    /// Get a single enum value by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SystemEnumValueDto>> GetById(Guid id)
    {
        var result = await _enumService.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Create a new enum value
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SystemEnumValueDto>> Create([FromBody] CreateSystemEnumValueDto dto)
    {
        try
        {
            var result = await _enumService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an enum value
    /// </summary>
    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<SystemEnumValueDto>> Update(Guid id, [FromBody] UpdateSystemEnumValueDto dto)
    {
        var result = await _enumService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Toggle active status of an enum value
    /// </summary>
    [HttpPost("{id:guid}/toggle-active")]
    public async Task<ActionResult> ToggleActive(Guid id)
    {
        try
        {
            var success = await _enumService.ToggleActiveAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an enum value (soft delete)
    /// </summary>
    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var success = await _enumService.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reorder enum values
    /// </summary>
    [HttpPost("reorder")]
    public async Task<ActionResult> Reorder([FromBody] ReorderEnumValuesDto dto)
    {
        var success = await _enumService.ReorderAsync(dto);
        return success ? NoContent() : BadRequest();
    }

    /// <summary>
    /// Seed default enum values
    /// </summary>
    [HttpPost("seed")]
    public async Task<ActionResult> SeedDefaults()
    {
        await _enumService.SeedDefaultValuesAsync();
        return Ok(new { message = "Default enum values seeded successfully" });
    }
}
