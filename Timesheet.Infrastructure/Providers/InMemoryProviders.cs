namespace Timesheet.Infrastructure.Providers;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class InMemoryEmployeeProvider : IEmployeeProvider
{
    private static List<EmployeeRecord> employees = new();
    public Task<IEnumerable<EmployeeRecord>> GetAllAsync() => Task.FromResult(employees.AsEnumerable());
    public Task<EmployeeRecord> GetByIdAsync(string id) => Task.FromResult(employees.FirstOrDefault(e => e.EmployeeId == id));
    public Task AddAsync(EmployeeRecord emp) { employees.Add(emp); return Task.CompletedTask; }
}

public class InMemorySwipeProvider : ISwipeProvider
{
    private static List<SwipeRecord> swipes = new();
    public Task<IEnumerable<SwipeRecord>> GetAllAsync() => Task.FromResult(swipes.AsEnumerable());
    public Task<IEnumerable<SwipeRecord>> GetByEmployeeAsync(string empId) => Task.FromResult(swipes.Where(s => s.EmployeeId == empId).AsEnumerable());
    public Task<IEnumerable<SwipeRecord>> GetByDateRangeAsync(string empId, DateTime start, DateTime end) => Task.FromResult(swipes.Where(s => s.EmployeeId == empId && s.Date >= start && s.Date <= end).AsEnumerable());
    public Task AddAsync(SwipeRecord swipe) { swipes.Add(swipe); return Task.CompletedTask; }
}

public class InMemoryLeaveProvider : ILeaveProvider
{
    private static List<LeaveRecord> leaves = new();
    public Task<IEnumerable<LeaveRecord>> GetAllAsync() => Task.FromResult(leaves.AsEnumerable());
    public Task<IEnumerable<LeaveRecord>> GetByEmployeeAsync(string empId) => Task.FromResult(leaves.Where(l => l.EmployeeId == empId).AsEnumerable());
    public Task AddAsync(LeaveRecord leave) { leaves.Add(leave); return Task.CompletedTask; }
}

public class InMemoryHolidayProvider : IHolidayProvider
{
    private static List<HolidayRecord> holidays = new();
    public Task<IEnumerable<HolidayRecord>> GetAllAsync() => Task.FromResult(holidays.AsEnumerable());
    public Task<HolidayRecord> GetByDateAsync(DateTime date) => Task.FromResult(holidays.FirstOrDefault(h => h.Date.Date == date.Date));
    public Task AddAsync(HolidayRecord holiday) { holidays.Add(holiday); return Task.CompletedTask; }
}

public class InMemorySettingsProvider : ISettingsProvider
{
    private static readonly IntegrationSettings settings = new();

    public Task<IntegrationSettings> GetSettingsAsync() => Task.FromResult(settings);

    public Task UpdateSettingsAsync(string dataType, DataSourceSettings updated)
    {
        switch (dataType.ToLowerInvariant())
        {
            case "employee": settings.Employee = updated; break;
            case "swipe": settings.Swipe = updated; break;
            case "leave": settings.Leave = updated; break;
            case "holiday": settings.Holiday = updated; break;
            default: throw new ArgumentException($"Unknown data type: {dataType}");
        }
        return Task.CompletedTask;
    }
}
