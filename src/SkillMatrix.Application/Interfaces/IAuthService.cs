using SkillMatrix.Application.DTOs.Auth;

namespace SkillMatrix.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<List<UserDto>> GetAllUsersAsync();
    Task SeedDefaultUsersAsync();
}
