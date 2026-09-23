namespace Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
}

public interface IEmployeeProvider
{
    Task<IEnumerable<EmployeeRecord>> GetAllAsync();
    Task<EmployeeRecord> GetByIdAsync(string employeeId);
    Task AddAsync(EmployeeRecord employee);
}

public interface ISwipeProvider
{
    Task<IEnumerable<SwipeRecord>> GetAllAsync();
    Task<IEnumerable<SwipeRecord>> GetByEmployeeAsync(string employeeId);
    Task<IEnumerable<SwipeRecord>> GetByDateRangeAsync(string employeeId, DateTime start, DateTime end);
    Task AddAsync(SwipeRecord swipe);
}

public interface ILeaveProvider
{
    Task<IEnumerable<LeaveRecord>> GetAllAsync();
    Task<IEnumerable<LeaveRecord>> GetByEmployeeAsync(string employeeId);
    Task AddAsync(LeaveRecord leave);
}

public interface IHolidayProvider
{
    Task<IEnumerable<HolidayRecord>> GetAllAsync();
    Task<HolidayRecord> GetByDateAsync(DateTime date);
    Task AddAsync(HolidayRecord holiday);
}

public interface ISettingsProvider
{
    Task<IntegrationSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(string dataType, DataSourceSettings settings);
}

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task<AuthResult> LoginWithSsoAsync();
    Task<AuthResult> RefreshAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
}
