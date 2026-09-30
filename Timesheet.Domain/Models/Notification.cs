namespace Timesheet.Domain.Models;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public string ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}

public enum NotificationType
{
    TimesheetReminder,
    ApprovalStatus,
    LeaveApproval,
    SwipeDiscrepancy,
    LeaveBalanceLow,
    SystemAlert,
    General
}

public enum NotificationStatus
{
    Unread,
    Read,
    Archived
}

public class NotificationSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool TimesheetReminders { get; set; } = true;
    public bool ApprovalNotifications { get; set; } = true;
    public bool LeaveNotifications { get; set; } = true;
    public int ReminderDaysBefore { get; set; } = 1;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}
