namespace Timesheet.Domain.Interfaces;

public interface IMongoDbService
{
    Task<T> GetByIdAsync<T>(string id) where T : class;
    Task<List<T>> GetAllAsync<T>() where T : class;
    Task<List<T>> GetAsync<T>(Func<T, bool> predicate) where T : class;
    Task<T> InsertAsync<T>(T entity) where T : class;
    Task UpdateAsync<T>(string id, T entity) where T : class;
    Task DeleteAsync<T>(string id) where T : class;
    Task<long> CountAsync<T>(Func<T, bool> predicate) where T : class;
}

public interface ILeaveRequestRepository
{
    Task<LeaveRequest> CreateAsync(LeaveRequest request);
    Task<LeaveRequest> GetByIdAsync(string id);
    Task<List<LeaveRequest>> GetByEmployeeIdAsync(string employeeId);
    Task<List<LeaveRequest>> GetPendingApprovalsAsync();
    Task UpdateAsync(LeaveRequest request);
    Task DeleteAsync(string id);
}

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance> GetBalanceAsync(string employeeId, string leaveType, int year);
    Task UpdateBalanceAsync(LeaveBalance balance);
    Task<List<LeaveBalance>> GetEmployeeBalancesAsync(string employeeId);
}

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<List<Notification>> GetUnreadAsync(string employeeId);
    Task<List<Notification>> GetByEmployeeIdAsync(string employeeId, int limit = 50);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string employeeId);
    Task DeleteAsync(string notificationId);
}

public interface ISettingsRepository
{
    Task<CompanySettings> GetCompanySettingsAsync();
    Task UpdateCompanySettingsAsync(CompanySettings settings);
    Task<EmployeeSettings> GetEmployeeSettingsAsync(string employeeId);
    Task UpdateEmployeeSettingsAsync(EmployeeSettings settings);
}

public interface IAuditLogRepository
{
    Task LogAsync(AuditLog log);
    Task<List<AuditLog>> GetByEmployeeIdAsync(string employeeId);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId);
}

public interface IAnalyticsRepository
{
    Task SaveTimesheetAnalyticsAsync(TimesheetAnalytics analytics);
    Task SaveDepartmentAnalyticsAsync(DepartmentAnalytics analytics);
    Task SaveAnomalyAsync(AttendanceAnomalies anomaly);
    Task<TimesheetAnalytics> GetLatestAnalyticsAsync(string employeeId);
    Task<List<AttendanceAnomalies>> GetAnomaliesAsync(string employeeId);
}
