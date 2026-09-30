namespace Timesheet.Infrastructure.Repositories;

using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly IMongoCollection<LeaveRequest> _collection;
    private readonly ILogger<LeaveRequestRepository> _logger;

    public LeaveRequestRepository(IMongoDatabase database, ILogger<LeaveRequestRepository> logger)
    {
        _collection = database.GetCollection<LeaveRequest>("LeaveRequests");
        _logger = logger;
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest request)
    {
        try
        {
            await _collection.InsertOneAsync(request);
            _logger.LogInformation($"Created leave request {request.Id} for employee {request.EmployeeId}");
            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating leave request: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<LeaveRequest> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting leave request {id}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<LeaveRequest>> GetByEmployeeIdAsync(string employeeId)
    {
        try
        {
            var filter = Builders<LeaveRequest>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting leave requests for employee {employeeId}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<LeaveRequest>> GetPendingApprovalsAsync()
    {
        try
        {
            var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Status, LeaveRequestStatus.Pending);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting pending leave requests: {ex.Message}", ex);
            throw;
        }
    }

    public async Task UpdateAsync(LeaveRequest request)
    {
        try
        {
            var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Id, request.Id);
            request.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(filter, request);
            _logger.LogInformation($"Updated leave request {request.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating leave request {request.Id}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Id, id);
            await _collection.DeleteOneAsync(filter);
            _logger.LogInformation($"Deleted leave request {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting leave request {id}: {ex.Message}", ex);
            throw;
        }
    }
}
