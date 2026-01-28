using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Auth;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly SkillMatrixDbContext _context;

    public AuthController(IAuthService authService, SkillMatrixDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    /// <summary>
    /// User login
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
    /// Register new user
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
    /// Get current user information
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
    /// Change password
    /// </summary>
    [HttpPost("change-password/{userId}")]
    public async Task<ActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request)
    {
        var success = await _authService.ChangePasswordAsync(userId, request);
        if (!success)
            return BadRequest(new { error = "Current password is incorrect" });
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Get user profile
    /// </summary>
    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetProfile(Guid userId)
    {
        var profile = await _authService.GetProfileAsync(userId);
        if (profile == null)
            return NotFound(new { error = "User not found" });
        return Ok(profile);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPost("profile/{userId}/update")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateProfile(Guid userId, [FromBody] UpdateProfileRequest request)
    {
        var profile = await _authService.UpdateProfileAsync(userId, request);
        if (profile == null)
            return NotFound(new { error = "User not found" });
        return Ok(profile);
    }

    /// <summary>
    /// Get all users (admin only)
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

    /// <summary>
    /// Run database migration for AI grading fields (temporary)
    /// </summary>
    [HttpPost("migrate-ai-fields")]
    public async Task<ActionResult> MigrateAiFields()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""Questions"" ADD COLUMN IF NOT EXISTS ""ExpectedAnswer"" TEXT;
                ALTER TABLE ""AssessmentResponses"" ADD COLUMN IF NOT EXISTS ""AiFeedback"" TEXT;
            ");
            return Ok(new { message = "Migration completed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
