using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Auth;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Organization;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace SkillMatrix.Application.Services;

public class AuthService : IAuthService
{
    private readonly SkillMatrixDbContext _context;

    public AuthService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == request.Email.ToLower() && !e.IsDeleted);

        if (employee == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Email does not exist in the system"
            };
        }

        if (!VerifyPassword(request.Password, employee.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Incorrect password"
            };
        }

        if (employee.Status != EmploymentStatus.Active)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Account has been disabled"
            };
        }

        var userDto = MapToUserDto(employee);
        var token = GenerateSimpleToken(employee.Id);

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            User = userDto,
            Token = token
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email exists
        var exists = await _context.Employees
            .AnyAsync(e => e.Email.ToLower() == request.Email.ToLower() && !e.IsDeleted);

        if (exists)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Email is already in use"
            };
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            SystemRole = request.Role,
            TeamId = request.TeamId,
            Status = EmploymentStatus.Active,
            JoinDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .FirstAsync(e => e.Id == employee.Id);

        var userDto = MapToUserDto(employee);
        var token = GenerateSimpleToken(employee.Id);

        return new LoginResponse
        {
            Success = true,
            Message = "Registration successful",
            User = userDto,
            Token = token
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .FirstOrDefaultAsync(e => e.Id == userId && !e.IsDeleted);

        return employee != null ? MapToUserDto(employee) : null;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == userId && !e.IsDeleted);

        if (employee == null)
            return false;

        if (!VerifyPassword(request.CurrentPassword, employee.PasswordHash))
            return false;

        employee.PasswordHash = HashPassword(request.NewPassword);
        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Where(e => !e.IsDeleted && e.Status == EmploymentStatus.Active)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        return employees.Select(MapToUserDto).ToList();
    }

    public async Task<EmployeeProfileDto?> GetProfileAsync(Guid userId)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Include(e => e.Manager)
            .Include(e => e.Skills)
            .Include(e => e.Assessments)
            .FirstOrDefaultAsync(e => e.Id == userId && !e.IsDeleted);

        if (employee == null)
            return null;

        return MapToProfileDto(employee);
    }

    public async Task<EmployeeProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Include(e => e.Manager)
            .Include(e => e.Skills)
            .Include(e => e.Assessments)
            .FirstOrDefaultAsync(e => e.Id == userId && !e.IsDeleted);

        if (employee == null)
            return null;

        employee.FullName = request.FullName;
        employee.AvatarUrl = request.AvatarUrl;
        employee.JoinDate = request.JoinDate;
        employee.YearsOfExperience = request.YearsOfExperience;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToProfileDto(employee);
    }

    public async Task SeedDefaultUsersAsync()
    {
        // Check if demo users already exist (by email)
        var hasAdminUser = await _context.Employees
            .AnyAsync(e => e.Email == "admin@skillmatrix.com" && !e.IsDeleted);
        if (hasAdminUser) return;

        var adminId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var emp1Id = Guid.NewGuid();
        var emp2Id = Guid.NewGuid();

        var defaultUsers = new List<Employee>
        {
            new Employee
            {
                Id = adminId,
                UserId = adminId.ToString(),
                Email = "admin@skillmatrix.com",
                PasswordHash = HashPassword("admin123"),
                FullName = "System Admin",
                SystemRole = UserRole.Admin,
                Status = EmploymentStatus.Active,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = managerId,
                UserId = managerId.ToString(),
                Email = "manager@skillmatrix.com",
                PasswordHash = HashPassword("manager123"),
                FullName = "Team Manager",
                SystemRole = UserRole.Manager,
                Status = EmploymentStatus.Active,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = emp1Id,
                UserId = emp1Id.ToString(),
                Email = "employee1@skillmatrix.com",
                PasswordHash = HashPassword("employee123"),
                FullName = "John Doe",
                SystemRole = UserRole.Employee,
                Status = EmploymentStatus.Active,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = emp2Id,
                UserId = emp2Id.ToString(),
                Email = "employee2@skillmatrix.com",
                PasswordHash = HashPassword("employee123"),
                FullName = "Jane Smith",
                SystemRole = UserRole.Employee,
                Status = EmploymentStatus.Active,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Employees.AddRange(defaultUsers);
        await _context.SaveChangesAsync();
    }

    private static UserDto MapToUserDto(Employee employee)
    {
        return new UserDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FullName = employee.FullName,
            AvatarUrl = employee.AvatarUrl,
            Role = employee.SystemRole,
            RoleName = employee.SystemRole.ToString(),
            TeamId = employee.TeamId,
            TeamName = employee.Team?.Name,
            JobRoleId = employee.JobRoleId,
            JobRoleName = employee.JobRole?.Name
        };
    }

    private static EmployeeProfileDto MapToProfileDto(Employee employee)
    {
        var completedAssessments = employee.Assessments?
            .Count(a => a.Status == AssessmentStatus.Completed || a.Status == AssessmentStatus.Reviewed) ?? 0;

        var skills = employee.Skills?.ToList() ?? new List<EmployeeSkill>();
        var avgLevel = skills.Any() ? skills.Average(s => (int)s.CurrentLevel) : 0;

        return new EmployeeProfileDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FullName = employee.FullName,
            AvatarUrl = employee.AvatarUrl,
            Role = employee.SystemRole,
            RoleName = employee.SystemRole.ToString(),
            Status = employee.Status,
            StatusName = employee.Status.ToString(),
            TeamId = employee.TeamId,
            TeamName = employee.Team?.Name,
            JobRoleId = employee.JobRoleId,
            JobRoleName = employee.JobRole?.Name,
            ManagerId = employee.ManagerId,
            ManagerName = employee.Manager?.FullName,
            JoinDate = employee.JoinDate,
            YearsOfExperience = employee.YearsOfExperience,
            TotalSkills = skills.Count,
            CompletedAssessments = completedAssessments,
            AverageSkillLevel = Math.Round(avgLevel, 1),
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "SkillMatrix_Salt_2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private static string GenerateSimpleToken(Guid userId)
    {
        // Simple token for demo - in production use JWT
        var data = $"{userId}|{DateTime.UtcNow:O}";
        var bytes = Encoding.UTF8.GetBytes(data);
        return Convert.ToBase64String(bytes);
    }
}
