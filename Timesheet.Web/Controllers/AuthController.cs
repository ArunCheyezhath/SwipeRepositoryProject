namespace Timesheet.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);
        return result.Success ? Ok(result) : Unauthorized(new { message = result.Error });
    }

    [HttpPost("sso")]
    [AllowAnonymous]
    public async Task<IActionResult> Sso()
    {
        var result = await _authService.LoginWithSsoAsync();
        return result.Success ? Ok(result) : Unauthorized(new { message = result.Error });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        return result.Success ? Ok(result) : Unauthorized(new { message = result.Error });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.RevokeAsync(request.RefreshToken);
        return Ok(new { message = "Logged out" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.FindFirst("sub")?.Value,
            employeeId = User.FindFirst("employeeId")?.Value,
            displayName = User.FindFirst("displayName")?.Value
        });
    }
}
