namespace Timesheet.Infrastructure.Services;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class CsvImportService : IFileImportService
{
    private readonly IEmployeeProvider _employeeProvider;
    private readonly ISwipeProvider _swipeProvider;
    private readonly ILeaveProvider _leaveProvider;
    private readonly IHolidayProvider _holidayProvider;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions ApiJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CsvImportService(
        IEmployeeProvider emp,
        ISwipeProvider swipe,
        ILeaveProvider leave,
        IHolidayProvider holiday,
        ISettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory)
    {
        _employeeProvider = emp;
        _swipeProvider = swipe;
        _leaveProvider = leave;
        _holidayProvider = holiday;
        _settingsProvider = settingsProvider;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ValidationResult> ImportEmployeesAsync(Stream csvStream, string employeeId)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<EmployeeRecord>().ToList();
            var matched = records.Where(r => r.EmployeeId == employeeId).ToList();
            foreach (var emp in matched)
                await _employeeProvider.AddAsync(emp);
            return new ValidationResult { IsValid = true, Message = $"Imported {matched.Count} employee record(s) for {employeeId} (skipped {records.Count - matched.Count} row(s) for other employees)" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> ImportSwipesAsync(Stream csvStream, string employeeId)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToList();
            var count = 0;
            foreach (var rec in records)
            {
                string rowEmployeeId = rec.EmployeeId;
                if (rowEmployeeId != employeeId) continue;

                string swipeTimeStr = rec.SwipeTime;
                string swipeTypeStr = rec.SwipeType;
                if (TimeSpan.TryParse(swipeTimeStr, out TimeSpan time) && Enum.TryParse<SwipeType>(swipeTypeStr, out SwipeType type))
                {
                    await _swipeProvider.AddAsync(new SwipeRecord
                    {
                        EmployeeId = rowEmployeeId,
                        Date = DateTime.Parse(rec.Date),
                        SwipeTime = time,
                        SwipeType = type
                    });
                    count++;
                }
            }
            return new ValidationResult { IsValid = true, Message = $"Imported {count} swipe record(s) for {employeeId}" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> ImportLeavesAsync(Stream csvStream, string employeeId)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToList();
            var count = 0;
            foreach (var rec in records)
            {
                string rowEmployeeId = rec.EmployeeId;
                if (rowEmployeeId != employeeId) continue;

                string leaveStatusStr = rec.Status;
                if (Enum.TryParse<LeaveStatus>(leaveStatusStr, out LeaveStatus status))
                {
                    await _leaveProvider.AddAsync(new LeaveRecord
                    {
                        EmployeeId = rowEmployeeId,
                        Date = DateTime.Parse(rec.Date),
                        LeaveType = rec.LeaveType,
                        Status = status
                    });
                    count++;
                }
            }
            return new ValidationResult { IsValid = true, Message = $"Imported {count} leave record(s) for {employeeId}" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> ImportHolidaysAsync(Stream csvStream)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;
            foreach (var rec in records)
            {
                await _holidayProvider.AddAsync(new HolidayRecord
                {
                    Date = DateTime.Parse(rec.Date),
                    HolidayName = rec.HolidayName
                });
                count++;
            }
            return new ValidationResult { IsValid = true, Message = $"Imported {count} holidays" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> SyncEmployeeFromApiAsync(string employeeId)
    {
        try
        {
            var settings = (await _settingsProvider.GetSettingsAsync()).Employee;
            var records = await FetchFromApiAsync<EmployeeRecord>(settings);
            var matched = records.Where(r => r.EmployeeId == employeeId).ToList();
            foreach (var emp in matched)
                await _employeeProvider.AddAsync(emp);
            return new ValidationResult { IsValid = true, Message = $"Synced {matched.Count} employee record(s) for {employeeId} from API" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> SyncSwipesFromApiAsync(string employeeId)
    {
        try
        {
            var settings = (await _settingsProvider.GetSettingsAsync()).Swipe;
            var records = await FetchFromApiAsync<SwipeRecord>(settings);
            var matched = records.Where(r => r.EmployeeId == employeeId).ToList();
            foreach (var swipe in matched)
                await _swipeProvider.AddAsync(swipe);
            return new ValidationResult { IsValid = true, Message = $"Synced {matched.Count} swipe record(s) for {employeeId} from API" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> SyncLeavesFromApiAsync(string employeeId)
    {
        try
        {
            var settings = (await _settingsProvider.GetSettingsAsync()).Leave;
            var records = await FetchFromApiAsync<LeaveRecord>(settings);
            var matched = records.Where(r => r.EmployeeId == employeeId).ToList();
            foreach (var leave in matched)
                await _leaveProvider.AddAsync(leave);
            return new ValidationResult { IsValid = true, Message = $"Synced {matched.Count} leave record(s) for {employeeId} from API" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    public async Task<ValidationResult> SyncHolidaysFromApiAsync()
    {
        try
        {
            var settings = (await _settingsProvider.GetSettingsAsync()).Holiday;
            var records = await FetchFromApiAsync<HolidayRecord>(settings);
            foreach (var holiday in records)
                await _holidayProvider.AddAsync(holiday);
            return new ValidationResult { IsValid = true, Message = $"Synced {records.Count} holiday(s) from API" };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Message = ex.Message };
        }
    }

    private async Task<List<T>> FetchFromApiAsync<T>(DataSourceSettings settings)
    {
        if (settings.SourceType != DataSourceType.Api)
            throw new InvalidOperationException("This data type is not configured to use a third-party API. Update it in Settings.");
        if (string.IsNullOrWhiteSpace(settings.ApiUrl))
            throw new InvalidOperationException("No API URL configured for this data type. Update it in Settings.");

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, settings.ApiUrl);
        if (!string.IsNullOrWhiteSpace(settings.ApiToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiToken);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<T>>(json, ApiJsonOptions) ?? new List<T>();
    }
}
