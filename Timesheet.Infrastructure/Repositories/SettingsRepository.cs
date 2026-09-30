namespace Timesheet.Infrastructure.Repositories;

using MongoDB.Driver;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class SettingsRepository : ISettingsRepository
{
    private readonly IMongoCollection<CompanySettings> _companyCollection;
    private readonly IMongoCollection<EmployeeSettings> _employeeCollection;
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(IMongoDatabase database, ILogger<SettingsRepository> logger)
    {
        _companyCollection = database.GetCollection<CompanySettings>("CompanySettings");
        _employeeCollection = database.GetCollection<EmployeeSettings>("EmployeeSettings");
        _logger = logger;
    }

    public async Task<CompanySettings> GetCompanySettingsAsync()
    {
        try
        {
            return await _companyCollection.Find(_ => true).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting company settings: {ex.Message}", ex);
            throw;
        }
    }

    public async Task UpdateCompanySettingsAsync(CompanySettings settings)
    {
        try
        {
            var filter = Builders<CompanySettings>.Filter.Eq(x => x.CompanyId, settings.CompanyId);
            settings.UpdatedAt = DateTime.UtcNow;
            await _companyCollection.ReplaceOneAsync(filter, settings, new ReplaceOptions { IsUpsert = true });
            _logger.LogInformation($"Updated company settings");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating company settings: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<EmployeeSettings> GetEmployeeSettingsAsync(string employeeId)
    {
        try
        {
            var filter = Builders<EmployeeSettings>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _employeeCollection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting employee settings: {ex.Message}", ex);
            throw;
        }
    }

    public async Task UpdateEmployeeSettingsAsync(EmployeeSettings settings)
    {
        try
        {
            var filter = Builders<EmployeeSettings>.Filter.Eq(x => x.EmployeeId, settings.EmployeeId);
            settings.UpdatedAt = DateTime.UtcNow;
            await _employeeCollection.ReplaceOneAsync(filter, settings, new ReplaceOptions { IsUpsert = true });
            _logger.LogInformation($"Updated settings for employee {settings.EmployeeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating employee settings: {ex.Message}", ex);
            throw;
        }
    }
}
