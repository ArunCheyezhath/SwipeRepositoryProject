namespace Timesheet.Domain.Models;

public enum LeaveStatus { Pending, Approved, Rejected, Cancelled }

public class LeaveRecord
{
    public string EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string LeaveType { get; set; }
    public LeaveStatus Status { get; set; }
}
