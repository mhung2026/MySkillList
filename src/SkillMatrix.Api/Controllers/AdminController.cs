using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly DatabaseSeeder _seeder;

    public AdminController(DatabaseSeeder seeder)
    {
        _seeder = seeder;
    }

    /// <summary>
    /// Reseed skill taxonomy with latest SFIA 9 data
    /// WARNING: This will clear all existing skill domains, subcategories, and skills
    /// </summary>
    [HttpPost("reseed-taxonomy")]
    public async Task<ActionResult<ApiResponse<string>>> ReseedTaxonomy()
    {
        try
        {
            await _seeder.ReseedSkillTaxonomyAsync();
            return Ok(ApiResponse<string>.Ok("Skill taxonomy reseeded with SFIA 9 data successfully. 6 domains, 22 subcategories, 146 skills created."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Failed to reseed: {ex.Message}"));
        }
    }
}
