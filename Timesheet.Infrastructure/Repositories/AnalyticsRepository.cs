namespace Timesheet.Infrastructure.Repositories;

using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IMongoCollection<TimesheetAnalytics> _timesheetCollection;
    private readonly IMongoCollection<DepartmentAnalytics> _departmentCollection;
    private readonly IMongoCollection<AttendanceAnomalies> _anomaliesCollection;
    private readonly ILogger<AnalyticsRepository> _logger;

    public AnalyticsRepository(IMongoDatabase database, ILogger<AnalyticsRepository> logger)
    {
        _timesheetCollection = database.GetCollection<TimesheetAnalytics>("TimesheetAnalytics");
        _departmentCollection = database.GetCollection<DepartmentAnalytics>("DepartmentAnalytics");
        _anomaliesCollection = database.GetCollection<AttendanceAnomalies>("AttendanceAnomalies");
        _logger = logger;
    }

    public async Task SaveTimesheetAnalyticsAsync(TimesheetAnalytics analytics)
    {
        try
        {
            var filter = Builders<TimesheetAnalytics>.Filter.And(
                Builders<TimesheetAnalytics>.Filter.Eq(x => x.EmployeeId, analytics.EmployeeId),
                Builders<TimesheetAnalytics>.Filter.Eq(x => x.Date, analytics.Date)
            );
            await _timesheetCollection.ReplaceOneAsync(filter, analytics, new ReplaceOptions { IsUpsert = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving timesheet analytics: {ex.Message}", ex);
            throw;
        }
    }

    public async Task SaveDepartmentAnalyticsAsync(DepartmentAnalytics analytics)
    {
        try
        {
            var filter = Builders<DepartmentAnalytics>.Filter.And(
                Builders<DepartmentAnalytics>.Filter.Eq(x => x.Department, analytics.Department),
                Builders<DepartmentAnalytics>.Filter.Eq(x => x.Date, analytics.Date)
            );
            await _departmentCollection.ReplaceOneAsync(filter, analytics, new ReplaceOptions { IsUpsert = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving department analytics: {ex.Message}", ex);
            throw;
        }
    }

    public async Task SaveAnomalyAsync(AttendanceAnomalies anomaly)
    {
        try
        {
            await _anomaliesCollection.InsertOneAsync(anomaly);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving anomaly: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<TimesheetAnalytics> GetLatestAnalyticsAsync(string employeeId)
    {
        try
        {
            var filter = Builders<TimesheetAnalytics>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _timesheetCollection.Find(filter).SortByDescending(x => x.Date).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting latest analytics: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<AttendanceAnomalies>> GetAnomaliesAsync(string employeeId)
    {
        try
        {
            var filter = Builders<AttendanceAnomalies>.Filter.And(
                Builders<AttendanceAnomalies>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<AttendanceAnomalies>.Filter.Eq(x => x.IsResolved, false)
            );
            return await _anomaliesCollection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting anomalies: {ex.Message}", ex);
            throw;
        }
    }
}
