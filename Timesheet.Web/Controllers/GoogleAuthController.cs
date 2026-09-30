namespace Timesheet.Web.Controllers;

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

[ApiController]
[Route("api/auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IEmployeeProvider _employeeProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthController> _logger;

    public GoogleAuthController(IEmployeeProvider employeeProvider, IConfiguration configuration, ILogger<GoogleAuthController> logger)
    {
        _employeeProvider = employeeProvider;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.IdToken))
                return BadRequest(new { message = "ID token is required" });

            // Verify Google token
            var googleClaimsPrincipal = await VerifyGoogleToken(request.IdToken);
            if (googleClaimsPrincipal == null)
                return Unauthorized(new { message = "Invalid Google token" });

            // Extract email from token
            var emailClaim = googleClaimsPrincipal.FindFirst("email");
            if (emailClaim == null)
                return BadRequest(new { message = "Email not found in token" });

            var email = emailClaim.Value;
            _logger.LogInformation($"Google login attempt for email: {email}");

            // Find or create employee by email
            var employees = await _employeeProvider.GetAllAsync();
            var employee = employees.FirstOrDefault(e => e.Email == email);

            if (employee == null)
            {
                _logger.LogWarning($"Employee not found for email: {email}");
                return NotFound(new { message = "Employee not found. Please contact administrator." });
            }

            // Generate JWT tokens
            var accessToken = GenerateAccessToken(employee.EmployeeId, employee.EmployeeName);
            var refreshToken = GenerateRefreshToken();

            return Ok(new
            {
                accessToken,
                refreshToken,
                employeeId = employee.EmployeeId,
                displayName = employee.EmployeeName,
                email = employee.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Google login error: {ex.Message}");
            return StatusCode(500, new { message = "Google login failed" });
        }
    }

    [HttpPost("sso")]
    public async Task<IActionResult> SsoLogin()
    {
        try
        {
            // SSO integration would go here
            // For now, return a demo employee
            var demoEmployee = new EmployeeRecord
            {
                EmployeeId = "sso_user_001",
                EmployeeName = "SSO User",
                Email = "sso@company.com",
                StandardHours = 8
            };

            var accessToken = GenerateAccessToken(demoEmployee.EmployeeId, demoEmployee.EmployeeName);
            var refreshToken = GenerateRefreshToken();

            return Ok(new
            {
                accessToken,
                refreshToken,
                employeeId = demoEmployee.EmployeeId,
                displayName = demoEmployee.EmployeeName,
                email = demoEmployee.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"SSO login error: {ex.Message}");
            return StatusCode(500, new { message = "SSO login failed" });
        }
    }

    private async Task<System.Security.Claims.ClaimsPrincipal> VerifyGoogleToken(string idToken)
    {
        try
        {
            // In production, verify the token signature with Google's public keys
            // For now, decode without verification (NOT for production!)
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);

            // Check token expiration
            if (token.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Google token has expired");
                return null;
            }

            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(token.Claims, "Google")
            );

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying Google token: {ex.Message}");
            return null;
        }
    }

    private string GenerateAccessToken(string employeeId, string displayName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", employeeId),
            new System.Security.Claims.Claim("name", displayName),
            new System.Security.Claims.Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenMinutes"] ?? "15")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; }
}
