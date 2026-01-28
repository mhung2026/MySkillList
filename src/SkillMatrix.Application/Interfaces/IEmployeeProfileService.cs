using SkillMatrix.Application.DTOs.Employee;

namespace SkillMatrix.Application.Interfaces;

public interface IEmployeeProfileService
{
    /// <summary>
    /// Get comprehensive skill profile for an employee
    /// </summary>
    Task<SkillProfileDto?> GetSkillProfileAsync(Guid employeeId);

    /// <summary>
    /// Get gap analysis comparing employee skills vs role requirements
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="targetRoleId">Target role ID (optional, defaults to current role)</param>
    Task<GapAnalysisDto?> GetGapAnalysisAsync(Guid employeeId, Guid? targetRoleId = null);

    /// <summary>
    /// Manually recalculate skill gaps for an employee
    /// </summary>
    Task<RecalculateGapsResultDto> RecalculateGapsAsync(Guid employeeId, Guid? targetRoleId = null);

    /// <summary>
    /// Bulk recalculate skill gaps for all employees in the system
    /// </summary>
    Task<BulkRecalculateGapsResultDto> BulkRecalculateGapsForAllEmployeesAsync();

    /// <summary>
    /// Create a learning path for skill development
    /// </summary>
    Task<LearningPathDto> CreateLearningPathAsync(Guid employeeId, CreateLearningPathRequest request);

    /// <summary>
    /// Get all learning paths for an employee
    /// </summary>
    Task<List<LearningPathDto>> GetLearningPathsAsync(Guid employeeId);
}
