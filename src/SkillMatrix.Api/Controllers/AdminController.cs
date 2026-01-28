using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Services;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly DatabaseSeeder _seeder;
    private readonly IDatabaseSeederService _seederService;
    private readonly SkillMatrixDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        DatabaseSeeder seeder,
        IDatabaseSeederService seederService,
        SkillMatrixDbContext context,
        IWebHostEnvironment environment)
    {
        _seeder = seeder;
        _seederService = seederService;
        _context = context;
        _environment = environment;
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

    /// <summary>
    /// Seed database with sample data (employees, roles, teams, skills, and skill gaps)
    /// This will create sample data if the database is empty
    /// </summary>
    [HttpPost("seed-sample-data")]
    public async Task<ActionResult<ApiResponse<SeedResultDto>>> SeedSampleData()
    {
        // TODO: Add admin authorization check
        var result = await _seederService.SeedSampleDataAsync();

        if (result.Success)
        {
            return Ok(ApiResponse<SeedResultDto>.Ok(result, "Sample data seeded successfully"));
        }

        return BadRequest(ApiResponse<SeedResultDto>.Fail(result.Message));
    }

    /// <summary>
    /// Run pending SQL migrations
    /// This will execute any SQL migration files from the Migrations folder
    /// </summary>
    [HttpPost("run-migrations")]
    public async Task<ActionResult<ApiResponse<string>>> RunMigrations()
    {
        try
        {
            var migrationsPath = Path.Combine(_environment.ContentRootPath, "..", "SkillMatrix.Infrastructure", "Persistence", "Migrations");
            var sqlFiles = new[]
            {
                "20260127_AddAiLearningPathFields.sql",
                "20260127_AddLearningRecommendations.sql"
            };

            var results = new List<string>();

            foreach (var sqlFile in sqlFiles)
            {
                var filePath = Path.Combine(migrationsPath, sqlFile);
                if (System.IO.File.Exists(filePath))
                {
                    var sql = await System.IO.File.ReadAllTextAsync(filePath);
                    await _context.Database.ExecuteSqlRawAsync(sql);
                    results.Add($"✓ Applied: {sqlFile}");
                }
                else
                {
                    results.Add($"⚠ Skipped (not found): {sqlFile}");
                }
            }

            return Ok(ApiResponse<string>.Ok(string.Join("\n", results), "Migrations completed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Failed to run migrations: {ex.Message}"));
        }
    }
}
