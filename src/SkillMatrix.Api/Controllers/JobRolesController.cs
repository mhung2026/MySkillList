using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/jobroles")]
public class JobRolesController : ControllerBase
{
    private readonly SkillMatrixDbContext _context;

    public JobRolesController(SkillMatrixDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all job roles for dropdown
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<List<DropdownItemDto>>>> GetJobRolesDropdown()
    {
        var jobRoles = await _context.JobRoles
            .Where(j => !j.IsDeleted && j.IsActive && j.IsCurrent)
            .OrderBy(j => j.LevelInHierarchy)
            .ThenBy(j => j.Name)
            .Select(j => new DropdownItemDto
            {
                Id = j.Id,
                Name = j.Name,
                Code = j.Code
            })
            .ToListAsync();

        return Ok(ApiResponse<List<DropdownItemDto>>.Ok(jobRoles));
    }
}
