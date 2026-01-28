using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly SkillMatrixDbContext _context;

    public TeamsController(SkillMatrixDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all teams for dropdown
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<List<DropdownItemDto>>>> GetTeamsDropdown()
    {
        var teams = await _context.Teams
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new DropdownItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = null
            })
            .ToListAsync();

        return Ok(ApiResponse<List<DropdownItemDto>>.Ok(teams));
    }
}
