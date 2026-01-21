using SkillMatrix.Application.DTOs.Dashboard;

namespace SkillMatrix.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync();
    Task<List<EmployeeSkillSummaryDto>> GetEmployeeSkillsAsync(Guid? teamId = null);
    Task<EmployeeSkillSummaryDto?> GetEmployeeSkillDetailAsync(Guid employeeId);
    Task<TeamSkillMatrixDto> GetTeamSkillMatrixAsync(Guid? teamId = null);
}
