namespace Timesheet.Domain.Models;

public enum WorkType { Office, WFH, Leave, Holiday, Weekend }
public enum EntryStatus { AutoFilled, Modified, Manual }

public class TimesheetEntry
{
    public string EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public WorkType WorkType { get; set; }
    public double Hours { get; set; }
    public EntryStatus Status { get; set; }
    public string Notes { get; set; }
    public TimeSpan? InTime { get; set; }
    public TimeSpan? OutTime { get; set; }
}

public class TimesheetSummary
{
    public string EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TimesheetEntry> Entries { get; set; }
    public double TotalHours { get; set; }
    public double OfficeHours { get; set; }
    public double WFHHours { get; set; }
    public double LeaveHours { get; set; }
    public double HolidayHours { get; set; }
    public double WeekendHours { get; set; }
    public int OfficeDays { get; set; }
    public int WFHDays { get; set; }
    public int LeaveDays { get; set; }
    public int HolidayDays { get; set; }
    public int WeekendDays { get; set; }
    public double ExpectedHours { get; set; }
    public double CompletionPercentage { get; set; }
}
