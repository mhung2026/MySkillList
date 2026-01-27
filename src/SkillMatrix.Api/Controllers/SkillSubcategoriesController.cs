using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillSubcategoriesController : ControllerBase
{
    private readonly ISkillSubcategoryService _service;

    public SkillSubcategoriesController(ISkillSubcategoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all subcategories with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SkillSubcategoryListDto>>>> GetAll(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? domainId = null)
    {
        var result = await _service.GetAllAsync(request, domainId);
        return Ok(ApiResponse<PagedResult<SkillSubcategoryListDto>>.Ok(result));
    }

    /// <summary>
    /// Get subcategory by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SkillSubcategoryDto>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<SkillSubcategoryDto>.Fail("Skill Subcategory not found"));

        return Ok(ApiResponse<SkillSubcategoryDto>.Ok(result));
    }

    /// <summary>
    /// Create new subcategory
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SkillSubcategoryDto>>> Create(
        [FromBody] CreateSkillSubcategoryDto dto)
    {
        // Check code uniqueness within domain
        if (await _service.CodeExistsAsync(dto.Code, dto.SkillDomainId))
            return BadRequest(ApiResponse<SkillSubcategoryDto>.Fail(
                $"Code '{dto.Code}' already exists in this domain"));

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SkillSubcategoryDto>.Ok(result, "Skill Subcategory created successfully"));
    }

    /// <summary>
    /// Update subcategory
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SkillSubcategoryDto>>> Update(
        Guid id, [FromBody] UpdateSkillSubcategoryDto dto)
    {
        // Check code uniqueness within domain
        if (await _service.CodeExistsAsync(dto.Code, dto.SkillDomainId, id))
            return BadRequest(ApiResponse<SkillSubcategoryDto>.Fail(
                $"Code '{dto.Code}' already exists in this domain"));

        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<SkillSubcategoryDto>.Ok(result, "Skill Subcategory updated successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SkillSubcategoryDto>.Fail("Skill Subcategory not found"));
        }
    }

    /// <summary>
    /// Soft delete subcategory
    /// </summary>
    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Skill Subcategory not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Skill Subcategory deleted successfully"));
    }

    /// <summary>
    /// Toggle active status
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid id)
    {
        var result = await _service.ToggleActiveAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Skill Subcategory not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Status toggled successfully"));
    }

    /// <summary>
    /// Get dropdown list
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<List<SkillSubcategoryDropdownDto>>>> GetDropdown(
        [FromQuery] Guid? domainId = null)
    {
        var result = await _service.GetDropdownAsync(domainId);
        return Ok(ApiResponse<List<SkillSubcategoryDropdownDto>>.Ok(result));
    }
}
