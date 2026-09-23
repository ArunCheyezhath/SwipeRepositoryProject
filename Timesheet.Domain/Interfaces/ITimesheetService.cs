namespace Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public interface ITimesheetCalculationService
{
    Task<TimesheetSummary> GenerateTimesheetAsync(string employeeId, DateTime startDate, DateTime endDate);
    Task<TimesheetEntry> CalculateEntryAsync(string employeeId, DateTime date);
    Task<List<TimesheetEntry>> RecalculateAsync(string employeeId, DateTime startDate, DateTime endDate);
}

public interface IFileImportService
{
    Task<ValidationResult> ImportEmployeesAsync(Stream csvStream, string employeeId);
    Task<ValidationResult> ImportSwipesAsync(Stream csvStream, string employeeId);
    Task<ValidationResult> ImportLeavesAsync(Stream csvStream, string employeeId);
    Task<ValidationResult> ImportHolidaysAsync(Stream csvStream);

    Task<ValidationResult> SyncEmployeeFromApiAsync(string employeeId);
    Task<ValidationResult> SyncSwipesFromApiAsync(string employeeId);
    Task<ValidationResult> SyncLeavesFromApiAsync(string employeeId);
    Task<ValidationResult> SyncHolidaysFromApiAsync();
}
