using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillDomainsController : ControllerBase
{
    private readonly ISkillDomainService _service;

    public SkillDomainsController(ISkillDomainService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all skill domains with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SkillDomainListDto>>>> GetAll(
        [FromQuery] PagedRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<SkillDomainListDto>>.Ok(result));
    }

    /// <summary>
    /// Get skill domain by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SkillDomainDto>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<SkillDomainDto>.Fail("Skill Domain not found"));

        return Ok(ApiResponse<SkillDomainDto>.Ok(result));
    }

    /// <summary>
    /// Create new skill domain
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SkillDomainDto>>> Create(
        [FromBody] CreateSkillDomainDto dto)
    {
        // Check code uniqueness
        if (await _service.CodeExistsAsync(dto.Code))
            return BadRequest(ApiResponse<SkillDomainDto>.Fail($"Code '{dto.Code}' already exists"));

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SkillDomainDto>.Ok(result, "Skill Domain created successfully"));
    }

    /// <summary>
    /// Update skill domain
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SkillDomainDto>>> Update(
        Guid id, [FromBody] UpdateSkillDomainDto dto)
    {
        // Check code uniqueness
        if (await _service.CodeExistsAsync(dto.Code, id))
            return BadRequest(ApiResponse<SkillDomainDto>.Fail($"Code '{dto.Code}' already exists"));

        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<SkillDomainDto>.Ok(result, "Skill Domain updated successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SkillDomainDto>.Fail("Skill Domain not found"));
        }
    }

    /// <summary>
    /// Soft delete skill domain (set IsDeleted = true)
    /// </summary>
    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Skill Domain not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Skill Domain deleted successfully"));
    }

    /// <summary>
    /// Toggle active status (activate/deactivate)
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid id)
    {
        var result = await _service.ToggleActiveAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Skill Domain not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Status toggled successfully"));
    }

    /// <summary>
    /// Get dropdown list for select inputs
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<List<SkillDomainDropdownDto>>>> GetDropdown()
    {
        var result = await _service.GetDropdownAsync();
        return Ok(ApiResponse<List<SkillDomainDropdownDto>>.Ok(result));
    }

    /// <summary>
    /// Check if code exists
    /// </summary>
    [HttpGet("check-code")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckCode(
        [FromQuery] string code, [FromQuery] Guid? excludeId = null)
    {
        var exists = await _service.CodeExistsAsync(code, excludeId);
        return Ok(ApiResponse<bool>.Ok(exists));
    }
}
