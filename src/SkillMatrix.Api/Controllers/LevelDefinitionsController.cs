using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Services;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LevelDefinitionsController : ControllerBase
{
    private readonly IProficiencyLevelDefinitionService _levelDefinitionService;

    public LevelDefinitionsController(IProficiencyLevelDefinitionService levelDefinitionService)
    {
        _levelDefinitionService = levelDefinitionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProficiencyLevelDefinitionDto>>> GetAll()
    {
        var levels = await _levelDefinitionService.GetAllAsync();
        return Ok(levels);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProficiencyLevelDefinitionDto>> GetById(Guid id)
    {
        var level = await _levelDefinitionService.GetByIdAsync(id);
        if (level == null)
            return NotFound();
        return Ok(level);
    }

    [HttpGet("by-level/{level:int}")]
    public async Task<ActionResult<ProficiencyLevelDefinitionDto>> GetByLevel(int level)
    {
        var levelDef = await _levelDefinitionService.GetByLevelAsync(level);
        if (levelDef == null)
            return NotFound();
        return Ok(levelDef);
    }

    [HttpPost]
    public async Task<ActionResult<ProficiencyLevelDefinitionDto>> Create(CreateProficiencyLevelDefinitionDto dto)
    {
        try
        {
            var level = await _levelDefinitionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = level.Id }, level);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<ProficiencyLevelDefinitionDto>> Update(Guid id, UpdateProficiencyLevelDefinitionDto dto)
    {
        try
        {
            var level = await _levelDefinitionService.UpdateAsync(id, dto);
            return Ok(level);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _levelDefinitionService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("seed")]
    public async Task<ActionResult> SeedDefaultLevels()
    {
        await _levelDefinitionService.SeedDefaultLevelsAsync();
        return Ok(new { message = "Default levels seeded successfully" });
    }
}
