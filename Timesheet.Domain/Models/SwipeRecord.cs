namespace Timesheet.Domain.Models;

public enum SwipeType { IN, OUT }

public class SwipeRecord
{
    public string EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan SwipeTime { get; set; }
    public SwipeType SwipeType { get; set; }
}
