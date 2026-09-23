namespace Timesheet.Domain.Models;

public class UserAccount
{
    public string Username { get; set; } = "";
    public string EmployeeId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = "";
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresInSeconds { get; set; }
    public string? EmployeeId { get; set; }
    public string? DisplayName { get; set; }
}
