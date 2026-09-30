namespace Timesheet.Domain.Models;

public class TimesheetAnalytics
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public int DaysWorked { get; set; }
    public int OfficeDays { get; set; }
    public int WfhDays { get; set; }
    public int LeaveDays { get; set; }
    public int HolidayDays { get; set; }
    public double TotalHours { get; set; }
    public double OfficeHours { get; set; }
    public double WfhHours { get; set; }
    public double LeaveHours { get; set; }
    public double AverageHoursPerDay { get; set; }
    public double OverTimeHours { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class DepartmentAnalytics
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Department { get; set; }
    public DateTime Date { get; set; }
    public int TotalEmployees { get; set; }
    public int PresentEmployees { get; set; }
    public int AbsentEmployees { get; set; }
    public int OnLeaveEmployees { get; set; }
    public double AverageHoursWorked { get; set; }
    public double CompliancePercentage { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AttendanceAnomalies
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string AnomalyType { get; set; }
    public string Description { get; set; }
    public AnomalySeverity Severity { get; set; }
    public bool IsResolved { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

public enum AnomalySeverity
{
    Info,
    Warning,
    Critical
}
