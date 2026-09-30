namespace Timesheet.Infrastructure.Repositories;

using MongoDB.Driver;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(IMongoDatabase database, ILogger<NotificationRepository> logger)
    {
        _collection = database.GetCollection<Notification>("Notifications");
        _logger = logger;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        try
        {
            await _collection.InsertOneAsync(notification);
            _logger.LogInformation($"Created notification for employee {notification.EmployeeId}");
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating notification: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<Notification>> GetUnreadAsync(string employeeId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<Notification>.Filter.Eq(x => x.Status, NotificationStatus.Unread)
            );
            return await _collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting unread notifications: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<Notification>> GetByEmployeeIdAsync(string employeeId, int limit = 50)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _collection.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting notifications for employee {employeeId}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.Id, notificationId);
            var update = Builders<Notification>.Update
                .Set(x => x.Status, NotificationStatus.Read)
                .Set(x => x.ReadAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(filter, update);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking notification as read: {ex.Message}", ex);
            throw;
        }
    }

    public async Task MarkAllAsReadAsync(string employeeId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<Notification>.Filter.Eq(x => x.Status, NotificationStatus.Unread)
            );
            var update = Builders<Notification>.Update
                .Set(x => x.Status, NotificationStatus.Read)
                .Set(x => x.ReadAt, DateTime.UtcNow);
            await _collection.UpdateManyAsync(filter, update);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking all notifications as read: {ex.Message}", ex);
            throw;
        }
    }

    public async Task DeleteAsync(string notificationId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.Id, notificationId);
            await _collection.DeleteOneAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting notification: {ex.Message}", ex);
            throw;
        }
    }
}
