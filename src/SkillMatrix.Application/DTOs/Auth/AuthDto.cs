using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.DTOs.Auth;

/// <summary>
/// Login request
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response with user info
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserDto? User { get; set; }
    public string? Token { get; set; }  // Simple token for demo
}

/// <summary>
/// User DTO for authenticated user
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
    public Guid? JobRoleId { get; set; }
    public string? JobRoleName { get; set; }
}

/// <summary>
/// Register request (for creating test users)
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public Guid? TeamId { get; set; }
}

/// <summary>
/// Change password request
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Employee Profile DTO - detailed profile information
/// </summary>
public class EmployeeProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public EmploymentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // Organization
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
    public Guid? JobRoleId { get; set; }
    public string? JobRoleName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }

    // Employment info
    public DateTime? JoinDate { get; set; }
    public int YearsOfExperience { get; set; }

    // Statistics
    public int TotalSkills { get; set; }
    public int CompletedAssessments { get; set; }
    public double AverageSkillLevel { get; set; }

    // Account info
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Update Profile request
/// </summary>
public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime? JoinDate { get; set; }
    public int YearsOfExperience { get; set; }
}
