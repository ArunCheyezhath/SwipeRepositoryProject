namespace Timesheet.Domain.Models;

public class CompanySettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyName { get; set; }
    public string CompanyId { get; set; }
    public double StandardWorkHoursPerDay { get; set; } = 8;
    public double StandardWorkHoursPerWeek { get; set; } = 40;
    public bool EnableWorkFromHome { get; set; } = true;
    public bool EnableLeaveManagement { get; set; } = true;
    public bool RequireApproval { get; set; } = true;
    public int ApprovalLevelCount { get; set; } = 1;
    public List<string> Holidays { get; set; } = new();
    public Dictionary<string, double> LeaveTypes { get; set; } = new()
    {
        { "Casual", 12 },
        { "Sick", 10 },
        { "Earned", 20 }
    };
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class EmployeeSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
    public bool NotificationsEnabled { get; set; } = true;
    public string ManagerId { get; set; }
    public string Department { get; set; }
    public DateTime? OnboardingDate { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; }
}
