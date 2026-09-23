namespace Timesheet.Infrastructure.Services;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class AuthService : IAuthService
{
    private static readonly List<UserAccount> Users = new();
    private static readonly ConcurrentDictionary<string, (string Username, DateTime ExpiresAt)> RefreshTokens = new();
    private static readonly object SeedLock = new();
    private static bool _seeded;

    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
        SeedDemoUsers();
    }

    private void SeedDemoUsers()
    {
        if (_seeded) return;
        lock (SeedLock)
        {
            if (_seeded) return;
            AddUser("alex.sharma", "EMP001", "Alex Sharma", "Password123!");
            AddUser("priya.nair", "EMP002", "Priya Nair", "Password123!");
            _seeded = true;
        }
    }

    private static void AddUser(string username, string employeeId, string displayName, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = HashPassword(password, salt);
        Users.Add(new UserAccount { Username = username, EmployeeId = employeeId, DisplayName = displayName, PasswordHash = hash, PasswordSalt = salt });
    }

    private static byte[] HashPassword(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);

    public Task<AuthResult> LoginAsync(string username, string password)
    {
        var user = Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        if (user == null)
            return Task.FromResult(new AuthResult { Success = false, Error = "Invalid username or password" });

        var computedHash = HashPassword(password, user.PasswordSalt);
        if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            return Task.FromResult(new AuthResult { Success = false, Error = "Invalid username or password" });

        return Task.FromResult(IssueTokens(user));
    }

    public Task<AuthResult> LoginWithSsoAsync()
    {
        // Simulated SSO: in a real integration this would validate an OIDC/SAML assertion from
        // the identity provider (Azure AD, Okta, Google Workspace, etc.) and map its claims to a
        // local user instead of picking a fixed demo account.
        var user = Users.First(u => u.Username == "alex.sharma");
        return Task.FromResult(IssueTokens(user));
    }

    public Task<AuthResult> RefreshAsync(string refreshToken)
    {
        if (!RefreshTokens.TryGetValue(refreshToken, out var entry) || entry.ExpiresAt < DateTime.UtcNow)
            return Task.FromResult(new AuthResult { Success = false, Error = "Refresh token is invalid or expired" });

        RefreshTokens.TryRemove(refreshToken, out _);

        var user = Users.FirstOrDefault(u => u.Username == entry.Username);
        if (user == null)
            return Task.FromResult(new AuthResult { Success = false, Error = "User no longer exists" });

        return Task.FromResult(IssueTokens(user));
    }

    public Task RevokeAsync(string refreshToken)
    {
        RefreshTokens.TryRemove(refreshToken, out _);
        return Task.CompletedTask;
    }

    private AuthResult IssueTokens(UserAccount user)
    {
        var accessMinutes = _config.GetValue<int>("Jwt:AccessTokenMinutes", 15);
        var refreshDays = _config.GetValue<int>("Jwt:RefreshTokenDays", 7);

        var accessToken = GenerateAccessToken(user, accessMinutes);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        RefreshTokens[refreshToken] = (user.Username, DateTime.UtcNow.AddDays(refreshDays));

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresInSeconds = accessMinutes * 60,
            EmployeeId = user.EmployeeId,
            DisplayName = user.DisplayName
        };
    }

    private string GenerateAccessToken(UserAccount user, int accessMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("employeeId", user.EmployeeId),
            new Claim("displayName", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
