namespace Timesheet.Infrastructure.Repositories;

using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(IMongoDatabase database, ILogger<AuditLogRepository> logger)
    {
        _collection = database.GetCollection<AuditLog>("AuditLogs");
        _logger = logger;
    }

    public async Task LogAsync(AuditLog log)
    {
        try
        {
            await _collection.InsertOneAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error logging audit: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetByEmployeeIdAsync(string employeeId)
    {
        try
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting audit logs: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        try
        {
            var filter = Builders<AuditLog>.Filter.And(
                Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType),
                Builders<AuditLog>.Filter.Eq(x => x.EntityId, entityId)
            );
            return await _collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting entity audit logs: {ex.Message}", ex);
            throw;
        }
    }
}
