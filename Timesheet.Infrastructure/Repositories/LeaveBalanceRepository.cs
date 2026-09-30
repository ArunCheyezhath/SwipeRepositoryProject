namespace Timesheet.Infrastructure.Repositories;

using MongoDB.Driver;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly IMongoCollection<LeaveBalance> _collection;
    private readonly ILogger<LeaveBalanceRepository> _logger;

    public LeaveBalanceRepository(IMongoDatabase database, ILogger<LeaveBalanceRepository> logger)
    {
        _collection = database.GetCollection<LeaveBalance>("LeaveBalances");
        _logger = logger;
    }

    public async Task<LeaveBalance> GetBalanceAsync(string employeeId, string leaveType, int year)
    {
        try
        {
            var filter = Builders<LeaveBalance>.Filter.And(
                Builders<LeaveBalance>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<LeaveBalance>.Filter.Eq(x => x.LeaveType, leaveType),
                Builders<LeaveBalance>.Filter.Eq(x => x.Year, year)
            );
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting leave balance: {ex.Message}", ex);
            throw;
        }
    }

    public async Task UpdateBalanceAsync(LeaveBalance balance)
    {
        try
        {
            var filter = Builders<LeaveBalance>.Filter.Eq(x => x.Id, balance.Id);
            balance.LastUpdated = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(filter, balance, new ReplaceOptions { IsUpsert = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating leave balance: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(string employeeId)
    {
        try
        {
            var filter = Builders<LeaveBalance>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting employee balances: {ex.Message}", ex);
            throw;
        }
    }
}
