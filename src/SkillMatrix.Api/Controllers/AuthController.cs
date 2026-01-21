using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Auth;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Đăng ký user mới
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin user hiện tại
    /// </summary>
    [HttpGet("me/{userId}")]
    public async Task<ActionResult<UserDto>> GetCurrentUser(Guid userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found" });
        return Ok(user);
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPost("change-password/{userId}")]
    public async Task<ActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request)
    {
        var success = await _authService.ChangePasswordAsync(userId, request);
        if (!success)
            return BadRequest(new { error = "Mật khẩu hiện tại không đúng" });
        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    /// <summary>
    /// Lấy danh sách tất cả users (for admin)
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Seed default users (for testing)
    /// </summary>
    [HttpPost("seed")]
    public async Task<ActionResult> SeedUsers()
    {
        await _authService.SeedDefaultUsersAsync();
        return Ok(new { message = "Default users created" });
    }
}
