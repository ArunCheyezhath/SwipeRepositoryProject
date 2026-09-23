namespace Timesheet.Infrastructure.Services;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class TimesheetCalculationService : ITimesheetCalculationService
{
    private readonly IEmployeeProvider _employeeProvider;
    private readonly ISwipeProvider _swipeProvider;
    private readonly ILeaveProvider _leaveProvider;
    private readonly IHolidayProvider _holidayProvider;

    public TimesheetCalculationService(IEmployeeProvider emp, ISwipeProvider swipe, ILeaveProvider leave, IHolidayProvider holiday)
    {
        _employeeProvider = emp;
        _swipeProvider = swipe;
        _leaveProvider = leave;
        _holidayProvider = holiday;
    }

    public async Task<TimesheetSummary> GenerateTimesheetAsync(string employeeId, DateTime startDate, DateTime endDate)
    {
        var entries = new List<TimesheetEntry>();
        var employee = await _employeeProvider.GetByIdAsync(employeeId);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var entry = await CalculateEntryAsync(employeeId, date);
            entries.Add(entry);
        }

        var standardHours = employee?.StandardHours ?? 8;
        var workingDays = entries.Count(e => e.WorkType != WorkType.Weekend);
        var totalHours = entries.Sum(e => e.Hours);
        var expectedHours = workingDays * standardHours;

        var summary = new TimesheetSummary
        {
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            Entries = entries,
            TotalHours = totalHours,
            OfficeHours = entries.Where(e => e.WorkType == WorkType.Office).Sum(e => e.Hours),
            WFHHours = entries.Where(e => e.WorkType == WorkType.WFH).Sum(e => e.Hours),
            LeaveHours = entries.Where(e => e.WorkType == WorkType.Leave).Sum(e => e.Hours),
            HolidayHours = entries.Where(e => e.WorkType == WorkType.Holiday).Sum(e => e.Hours),
            WeekendHours = entries.Where(e => e.WorkType == WorkType.Weekend).Sum(e => e.Hours),
            OfficeDays = entries.Count(e => e.WorkType == WorkType.Office),
            WFHDays = entries.Count(e => e.WorkType == WorkType.WFH),
            LeaveDays = entries.Count(e => e.WorkType == WorkType.Leave),
            HolidayDays = entries.Count(e => e.WorkType == WorkType.Holiday),
            WeekendDays = entries.Count(e => e.WorkType == WorkType.Weekend),
            ExpectedHours = expectedHours,
            CompletionPercentage = expectedHours > 0 ? Math.Round(totalHours / expectedHours * 100, 0) : 0
        };

        return summary;
    }

    public async Task<TimesheetEntry> CalculateEntryAsync(string employeeId, DateTime date)
    {
        var employee = await _employeeProvider.GetByIdAsync(employeeId);

        // Priority: Weekend > Holiday > Approved Leave > Swipe > WFH

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return new TimesheetEntry { EmployeeId = employeeId, Date = date, WorkType = WorkType.Weekend, Hours = 0, Status = EntryStatus.AutoFilled };

        var holiday = await _holidayProvider.GetByDateAsync(date);
        if (holiday != null)
            return new TimesheetEntry { EmployeeId = employeeId, Date = date, WorkType = WorkType.Holiday, Hours = 0, Status = EntryStatus.AutoFilled };

        var leaves = await _leaveProvider.GetByEmployeeAsync(employeeId);
        var leave = leaves.FirstOrDefault(l => l.Date.Date == date.Date && l.Status == LeaveStatus.Approved);
        if (leave != null)
            return new TimesheetEntry { EmployeeId = employeeId, Date = date, WorkType = WorkType.Leave, Hours = employee?.StandardHours ?? 8, Status = EntryStatus.AutoFilled, Notes = leave.LeaveType };

        var swipes = await _swipeProvider.GetByEmployeeAsync(employeeId);
        var daySwipes = swipes.Where(s => s.Date.Date == date.Date).OrderBy(s => s.SwipeTime).ToList();

        if (daySwipes.Any())
        {
            double hours = 0;
            for (int i = 0; i < daySwipes.Count - 1; i += 2)
            {
                if (daySwipes[i].SwipeType == SwipeType.IN && daySwipes[i + 1].SwipeType == SwipeType.OUT)
                {
                    hours += (daySwipes[i + 1].SwipeTime - daySwipes[i].SwipeTime).TotalHours;
                }
            }
            return new TimesheetEntry
            {
                EmployeeId = employeeId,
                Date = date,
                WorkType = WorkType.Office,
                Hours = hours,
                Status = EntryStatus.AutoFilled,
                InTime = daySwipes.First().SwipeTime,
                OutTime = daySwipes.Last().SwipeTime
            };
        }

        return new TimesheetEntry { EmployeeId = employeeId, Date = date, WorkType = WorkType.WFH, Hours = employee?.StandardHours ?? 8, Status = EntryStatus.AutoFilled, Notes = "No swipe record" };
    }

    public async Task<List<TimesheetEntry>> RecalculateAsync(string employeeId, DateTime startDate, DateTime endDate)
    {
        var entries = new List<TimesheetEntry>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
            entries.Add(await CalculateEntryAsync(employeeId, date));
        return entries;
    }
}
