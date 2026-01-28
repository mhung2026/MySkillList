using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Employee;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeProfileController : ControllerBase
{
    private readonly IEmployeeProfileService _service;
    private readonly SkillMatrixDbContext _context;

    public EmployeeProfileController(IEmployeeProfileService service, SkillMatrixDbContext context)
    {
        _service = service;
        _context = context;
    }

    /// <summary>
    /// Get comprehensive skill profile for an employee
    /// </summary>
    [HttpGet("{id:guid}/skill-profile")]
    public async Task<ActionResult<ApiResponse<SkillProfileDto>>> GetSkillProfile(Guid id)
    {
        // TODO: Add authorization check (id == current user)
        var result = await _service.GetSkillProfileAsync(id);
        if (result == null)
            return NotFound(ApiResponse<SkillProfileDto>.Fail("Employee not found"));
        return Ok(ApiResponse<SkillProfileDto>.Ok(result));
    }

    /// <summary>
    /// Get skill gap analysis comparing employee skills vs role requirements
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="targetRoleId">Optional target role ID (defaults to current role)</param>
    [HttpGet("{id:guid}/gap-analysis")]
    public async Task<ActionResult<ApiResponse<GapAnalysisDto>>> GetGapAnalysis(
        Guid id,
        [FromQuery] Guid? targetRoleId = null)
    {
        // TODO: Add authorization check (id == current user)
        var result = await _service.GetGapAnalysisAsync(id, targetRoleId);
        if (result == null)
            return NotFound(ApiResponse<GapAnalysisDto>.Fail("Employee not found"));
        return Ok(ApiResponse<GapAnalysisDto>.Ok(result));
    }

    /// <summary>
    /// Manually recalculate skill gaps for an employee
    /// </summary>
    [HttpPost("{id:guid}/gap-analysis/recalculate")]
    public async Task<ActionResult<ApiResponse<RecalculateGapsResultDto>>> RecalculateGaps(
        Guid id,
        [FromBody] RecalculateGapsRequest? request = null)
    {
        // TODO: Add authorization check (id == current user)
        var result = await _service.RecalculateGapsAsync(id, request?.TargetRoleId);
        return Ok(ApiResponse<RecalculateGapsResultDto>.Ok(result));
    }

    /// <summary>
    /// Bulk recalculate skill gaps for ALL employees in the system
    /// This is useful for initial setup or bulk data refresh
    /// </summary>
    [HttpPost("gap-analysis/recalculate-all")]
    public async Task<ActionResult<ApiResponse<BulkRecalculateGapsResultDto>>> BulkRecalculateGapsForAllEmployees()
    {
        // TODO: Add authorization check (admin only)
        var result = await _service.BulkRecalculateGapsForAllEmployeesAsync();
        return Ok(ApiResponse<BulkRecalculateGapsResultDto>.Ok(result));
    }

    /// <summary>
    /// Get AI-generated learning recommendations for skill gaps
    /// </summary>
    [HttpGet("{id:guid}/learning-recommendations")]
    public async Task<ActionResult<ApiResponse<List<LearningRecommendationDto>>>> GetLearningRecommendations(Guid id)
    {
        // TODO: Add authorization check (id == current user)
        var recommendations = await _context.LearningRecommendations
            .Include(lr => lr.SkillGap)
            .Where(lr => lr.SkillGap.EmployeeId == id && !lr.IsDeleted && !lr.IsCompleted)
            .OrderBy(lr => lr.SkillGap.Priority)
            .ThenBy(lr => lr.DisplayOrder)
            .Select(lr => new LearningRecommendationDto
            {
                Id = lr.Id,
                SkillGapId = lr.SkillGapId,
                SkillId = lr.SkillId,
                SkillName = lr.SkillName,
                RecommendationType = lr.RecommendationType,
                Title = lr.Title,
                Description = lr.Description,
                Url = lr.Url,
                EstimatedHours = lr.EstimatedHours,
                Rationale = lr.Rationale,
                DisplayOrder = lr.DisplayOrder,
                GeneratedAt = lr.GeneratedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<LearningRecommendationDto>>.Ok(recommendations));
    }

    /// <summary>
    /// Get all learning paths for an employee
    /// </summary>
    [HttpGet("{id:guid}/learning-paths")]
    public async Task<ActionResult<ApiResponse<List<LearningPathDto>>>> GetLearningPaths(Guid id)
    {
        // TODO: Add authorization check (id == current user)
        var result = await _service.GetLearningPathsAsync(id);
        return Ok(ApiResponse<List<LearningPathDto>>.Ok(result));
    }

    /// <summary>
    /// Create a learning path for skill development
    /// </summary>
    [HttpPost("{id:guid}/learning-path")]
    public async Task<ActionResult<ApiResponse<LearningPathDto>>> CreateLearningPath(
        Guid id,
        [FromBody] CreateLearningPathRequest request)
    {
        // TODO: Add authorization check (id == current user)
        try
        {
            var result = await _service.CreateLearningPathAsync(id, request);
            return CreatedAtAction(
                nameof(GetSkillProfile),
                new { id },
                ApiResponse<LearningPathDto>.Ok(result, "Learning path created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LearningPathDto>.Fail(ex.Message));
        }
    }
}
