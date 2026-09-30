namespace Timesheet.Domain.Interfaces;

public interface ILeaveManagementService
{
    Task<LeaveRequest> RequestLeaveAsync(string employeeId, DateTime startDate, DateTime endDate, string leaveType, string reason);
    Task<LeaveRequest> GetLeaveRequestAsync(string requestId);
    Task<List<LeaveRequest>> GetEmployeeLeaveRequestsAsync(string employeeId);
    Task<List<LeaveRequest>> GetPendingApprovalsAsync(string managerId);
    Task ApproveLeaveAsync(string requestId, string approvedBy);
    Task RejectLeaveAsync(string requestId, string approvedBy, string reason);
    Task CancelLeaveAsync(string requestId);
    Task<LeaveBalance> GetLeaveBalanceAsync(string employeeId, string leaveType);
    Task<Dictionary<string, LeaveBalance>> GetAllBalancesAsync(string employeeId);
    Task UpdateLeaveBalanceAsync(string employeeId, string leaveType, double daysUsed);
}

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(string employeeId, string title, string message, NotificationType type, string actionUrl = null);
    Task<List<Notification>> GetUnreadNotificationsAsync(string employeeId);
    Task<List<Notification>> GetNotificationHistoryAsync(string employeeId, int limit = 50);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string employeeId);
    Task SendTimesheetReminderAsync(string employeeId);
    Task SendApprovalNotificationAsync(string employeeId, string message);
    Task DeleteNotificationAsync(string notificationId);
    Task<NotificationSettings> GetNotificationSettingsAsync(string employeeId);
    Task UpdateNotificationSettingsAsync(string employeeId, NotificationSettings settings);
}

public interface IAnalyticsService
{
    Task GenerateTimesheetAnalyticsAsync(string employeeId, DateTime startDate, DateTime endDate);
    Task GenerateDepartmentAnalyticsAsync(string department, DateTime date);
    Task DetectAnomaliesAsync(string employeeId);
    Task<TimesheetAnalytics> GetEmployeeAnalyticsAsync(string employeeId);
    Task<List<AttendanceAnomalies>> GetAnomaliesAsync(string employeeId);
    Task<DepartmentAnalytics> GetDepartmentAnalyticsAsync(string department, DateTime date);
    Task<Dictionary<string, object>> GetDashboardInsightsAsync(string employeeId);
}

public interface ILogger
{
    void LogInfo(string message, Dictionary<string, object> metadata = null);
    void LogWarning(string message, Dictionary<string, object> metadata = null);
    void LogError(string message, Exception exception = null, Dictionary<string, object> metadata = null);
    void LogDebug(string message, Dictionary<string, object> metadata = null);
}

public interface IAuditService
{
    Task LogActionAsync(string employeeId, string action, string entityType, string entityId, object oldValue = null, object newValue = null, string ipAddress = null);
    Task<List<AuditLog>> GetAuditTrailAsync(string employeeId, int days = 30);
    Task<List<AuditLog>> GetEntityAuditTrailAsync(string entityType, string entityId);
}
