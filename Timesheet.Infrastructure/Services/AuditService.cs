namespace Timesheet.Infrastructure.Services;

using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepo;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository auditLogRepo, ILogger<AuditService> logger)
    {
        _auditLogRepo = auditLogRepo;
        _logger = logger;
    }

    public async Task LogActionAsync(string employeeId, string action, string entityType, string entityId, object oldValue = null, object newValue = null, string ipAddress = null)
    {
        try
        {
            var log = new AuditLog
            {
                EmployeeId = employeeId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepo.LogAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error logging audit: {ex.Message}", ex);
        }
    }

    public async Task<List<AuditLog>> GetAuditTrailAsync(string employeeId, int days = 30)
    {
        return await _auditLogRepo.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<List<AuditLog>> GetEntityAuditTrailAsync(string entityType, string entityId)
    {
        return await _auditLogRepo.GetByEntityAsync(entityType, entityId);
    }
}
