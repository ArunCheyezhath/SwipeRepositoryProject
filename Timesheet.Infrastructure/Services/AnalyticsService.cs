namespace Timesheet.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepo;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(IAnalyticsRepository analyticsRepo, ILogger<AnalyticsService> logger)
    {
        _analyticsRepo = analyticsRepo;
        _logger = logger;
    }

    public async Task GenerateTimesheetAnalyticsAsync(string employeeId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var analytics = new TimesheetAnalytics
            {
                EmployeeId = employeeId,
                Date = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _analyticsRepo.SaveTimesheetAnalyticsAsync(analytics);
            _logger.LogInformation($"Generated analytics for employee {employeeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating timesheet analytics: {ex.Message}", ex);
            throw;
        }
    }

    public async Task GenerateDepartmentAnalyticsAsync(string department, DateTime date)
    {
        try
        {
            var analytics = new DepartmentAnalytics
            {
                Department = department,
                Date = date,
                UpdatedAt = DateTime.UtcNow
            };

            await _analyticsRepo.SaveDepartmentAnalyticsAsync(analytics);
            _logger.LogInformation($"Generated department analytics for {department}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating department analytics: {ex.Message}", ex);
            throw;
        }
    }

    public async Task DetectAnomaliesAsync(string employeeId)
    {
        try
        {
            var anomaly = new AttendanceAnomalies
            {
                EmployeeId = employeeId,
                Date = DateTime.UtcNow,
                AnomalyType = "Detection",
                Description = "Anomaly detected",
                Severity = AnomalySeverity.Info,
                IsResolved = false
            };

            await _analyticsRepo.SaveAnomalyAsync(anomaly);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error detecting anomalies: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<TimesheetAnalytics> GetEmployeeAnalyticsAsync(string employeeId)
    {
        return await _analyticsRepo.GetLatestAnalyticsAsync(employeeId);
    }

    public async Task<List<AttendanceAnomalies>> GetAnomaliesAsync(string employeeId)
    {
        return await _analyticsRepo.GetAnomaliesAsync(employeeId);
    }

    public async Task<DepartmentAnalytics> GetDepartmentAnalyticsAsync(string department, DateTime date)
    {
        throw new NotImplementedException();
    }

    public async Task<Dictionary<string, object>> GetDashboardInsightsAsync(string employeeId)
    {
        try
        {
            var analytics = await _analyticsRepo.GetLatestAnalyticsAsync(employeeId);
            var anomalies = await _analyticsRepo.GetAnomaliesAsync(employeeId);

            return new Dictionary<string, object>
            {
                { "analytics", analytics },
                { "anomalies", anomalies },
                { "insightCount", anomalies.Count }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating dashboard insights: {ex.Message}", ex);
            throw;
        }
    }
}
