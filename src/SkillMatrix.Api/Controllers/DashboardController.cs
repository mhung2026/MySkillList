using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Dashboard;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get dashboard overview
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
    {
        var result = await _service.GetOverviewAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get skills list for all employees
    /// </summary>
    [HttpGet("employees/skills")]
    public async Task<ActionResult<List<EmployeeSkillSummaryDto>>> GetEmployeeSkills([FromQuery] Guid? teamId)
    {
        var result = await _service.GetEmployeeSkillsAsync(teamId);
        return Ok(result);
    }

    /// <summary>
    /// Get skill details for an employee
    /// </summary>
    [HttpGet("employees/{employeeId}/skills")]
    public async Task<ActionResult<EmployeeSkillSummaryDto>> GetEmployeeSkillDetail(Guid employeeId)
    {
        var result = await _service.GetEmployeeSkillDetailAsync(employeeId);
        if (result == null)
            return NotFound(new { error = "Employee not found" });
        return Ok(result);
    }

    /// <summary>
    /// Get team skill matrix
    /// </summary>
    [HttpGet("skill-matrix")]
    public async Task<ActionResult<TeamSkillMatrixDto>> GetSkillMatrix([FromQuery] Guid? teamId)
    {
        var result = await _service.GetTeamSkillMatrixAsync(teamId);
        return Ok(result);
    }
}
