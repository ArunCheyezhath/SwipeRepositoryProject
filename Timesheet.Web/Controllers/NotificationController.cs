namespace Timesheet.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Domain.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var notifications = await _notificationService.GetUnreadNotificationsAsync(employeeId);
            return Ok(new { count = notifications.Count, notifications });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting unread notifications: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetNotificationHistory(int limit = 50)
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var notifications = await _notificationService.GetNotificationHistoryAsync(employeeId, limit);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{notificationId}/mark-read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(new { message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            await _notificationService.MarkAllAsReadAsync(employeeId);
            return Ok(new { message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(string notificationId)
    {
        try
        {
            await _notificationService.DeleteNotificationAsync(notificationId);
            return Ok(new { message = "Notification deleted" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
