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
    /// Lấy tổng quan dashboard
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
    {
        var result = await _service.GetOverviewAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách skills của tất cả nhân viên
    /// </summary>
    [HttpGet("employees/skills")]
    public async Task<ActionResult<List<EmployeeSkillSummaryDto>>> GetEmployeeSkills([FromQuery] Guid? teamId)
    {
        var result = await _service.GetEmployeeSkillsAsync(teamId);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết skills của một nhân viên
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
    /// Lấy skill matrix của team
    /// </summary>
    [HttpGet("skill-matrix")]
    public async Task<ActionResult<TeamSkillMatrixDto>> GetSkillMatrix([FromQuery] Guid? teamId)
    {
        var result = await _service.GetTeamSkillMatrixAsync(teamId);
        return Ok(result);
    }
}
