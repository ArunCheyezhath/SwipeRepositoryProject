namespace Timesheet.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository notificationRepo, ILogger<NotificationService> logger)
    {
        _notificationRepo = notificationRepo;
        _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(string employeeId, string title, string message, NotificationType type, string actionUrl = null)
    {
        try
        {
            var notification = new Notification
            {
                EmployeeId = employeeId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl,
                Status = NotificationStatus.Unread,
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepo.CreateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating notification: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(string employeeId)
    {
        return await _notificationRepo.GetUnreadAsync(employeeId);
    }

    public async Task<List<Notification>> GetNotificationHistoryAsync(string employeeId, int limit = 50)
    {
        return await _notificationRepo.GetByEmployeeIdAsync(employeeId, limit);
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        await _notificationRepo.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadAsync(string employeeId)
    {
        await _notificationRepo.MarkAllAsReadAsync(employeeId);
    }

    public async Task SendTimesheetReminderAsync(string employeeId)
    {
        await CreateNotificationAsync(employeeId, "Timesheet Reminder", "Please submit your timesheet", NotificationType.TimesheetReminder);
    }

    public async Task SendApprovalNotificationAsync(string employeeId, string message)
    {
        await CreateNotificationAsync(employeeId, "Approval Update", message, NotificationType.ApprovalStatus);
    }

    public async Task DeleteNotificationAsync(string notificationId)
    {
        await _notificationRepo.DeleteAsync(notificationId);
    }

    public async Task<NotificationSettings> GetNotificationSettingsAsync(string employeeId)
    {
        throw new NotImplementedException("Implement via ISettingsRepository");
    }

    public async Task UpdateNotificationSettingsAsync(string employeeId, NotificationSettings settings)
    {
        throw new NotImplementedException("Implement via ISettingsRepository");
    }
}
