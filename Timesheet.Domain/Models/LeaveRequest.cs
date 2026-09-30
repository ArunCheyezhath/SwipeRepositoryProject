namespace Timesheet.Domain.Models;

public class LeaveRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; }
    public string Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public string ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum LeaveRequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public class LeaveBalance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public string LeaveType { get; set; }
    public double TotalDaysAllowed { get; set; }
    public double DaysUsed { get; set; }
    public double DaysRemaining { get; set; }
    public int Year { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
