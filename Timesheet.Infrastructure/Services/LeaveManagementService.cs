namespace Timesheet.Infrastructure.Services;

using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

public class LeaveManagementService : ILeaveManagementService
{
    private readonly ILeaveRequestRepository _leaveRequestRepo;
    private readonly ILeaveBalanceRepository _leaveBalanceRepo;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ILogger<LeaveManagementService> _logger;

    public LeaveManagementService(
        ILeaveRequestRepository leaveRequestRepo,
        ILeaveBalanceRepository leaveBalanceRepo,
        INotificationService notificationService,
        IAuditService auditService,
        ILogger<LeaveManagementService> logger)
    {
        _leaveRequestRepo = leaveRequestRepo;
        _leaveBalanceRepo = leaveBalanceRepo;
        _notificationService = notificationService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<LeaveRequest> RequestLeaveAsync(string employeeId, DateTime startDate, DateTime endDate, string leaveType, string reason)
    {
        try
        {
            var request = new LeaveRequest
            {
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
                LeaveType = leaveType,
                Reason = reason,
                Status = LeaveRequestStatus.Pending
            };

            var result = await _leaveRequestRepo.CreateAsync(request);

            await _auditService.LogActionAsync(employeeId, "LEAVE_REQUEST_CREATED", "LeaveRequest", request.Id);
            await _notificationService.CreateNotificationAsync(employeeId, "Leave Request Submitted", $"Your {leaveType} leave request has been submitted", NotificationType.LeaveApproval);

            _logger.LogInformation($"Leave request created for employee {employeeId}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error requesting leave: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<LeaveRequest> GetLeaveRequestAsync(string requestId)
    {
        return await _leaveRequestRepo.GetByIdAsync(requestId);
    }

    public async Task<List<LeaveRequest>> GetEmployeeLeaveRequestsAsync(string employeeId)
    {
        return await _leaveRequestRepo.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<List<LeaveRequest>> GetPendingApprovalsAsync(string managerId)
    {
        var allRequests = await _leaveRequestRepo.GetPendingApprovalsAsync();
        return allRequests;
    }

    public async Task ApproveLeaveAsync(string requestId, string approvedBy)
    {
        try
        {
            var request = await _leaveRequestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Leave request not found");

            request.Status = LeaveRequestStatus.Approved;
            request.ApprovedBy = approvedBy;
            request.ApprovedDate = DateTime.UtcNow;

            await _leaveRequestRepo.UpdateAsync(request);
            await _auditService.LogActionAsync(approvedBy, "LEAVE_APPROVED", "LeaveRequest", requestId);
            await _notificationService.CreateNotificationAsync(request.EmployeeId, "Leave Approved", $"Your leave request has been approved", NotificationType.LeaveApproval);

            _logger.LogInformation($"Leave request {requestId} approved by {approvedBy}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error approving leave: {ex.Message}", ex);
            throw;
        }
    }

    public async Task RejectLeaveAsync(string requestId, string approvedBy, string reason)
    {
        try
        {
            var request = await _leaveRequestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Leave request not found");

            request.Status = LeaveRequestStatus.Rejected;
            request.ApprovedBy = approvedBy;
            request.RejectionReason = reason;
            request.ApprovedDate = DateTime.UtcNow;

            await _leaveRequestRepo.UpdateAsync(request);
            await _auditService.LogActionAsync(approvedBy, "LEAVE_REJECTED", "LeaveRequest", requestId);
            await _notificationService.CreateNotificationAsync(request.EmployeeId, "Leave Rejected", $"Your leave request has been rejected: {reason}", NotificationType.LeaveApproval);

            _logger.LogInformation($"Leave request {requestId} rejected by {approvedBy}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error rejecting leave: {ex.Message}", ex);
            throw;
        }
    }

    public async Task CancelLeaveAsync(string requestId)
    {
        try
        {
            var request = await _leaveRequestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Leave request not found");

            request.Status = LeaveRequestStatus.Cancelled;
            await _leaveRequestRepo.UpdateAsync(request);
            await _auditService.LogActionAsync(request.EmployeeId, "LEAVE_CANCELLED", "LeaveRequest", requestId);

            _logger.LogInformation($"Leave request {requestId} cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cancelling leave: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<LeaveBalance> GetLeaveBalanceAsync(string employeeId, string leaveType)
    {
        return await _leaveBalanceRepo.GetBalanceAsync(employeeId, leaveType, DateTime.UtcNow.Year);
    }

    public async Task<Dictionary<string, LeaveBalance>> GetAllBalancesAsync(string employeeId)
    {
        var balances = await _leaveBalanceRepo.GetEmployeeBalancesAsync(employeeId);
        return balances.ToDictionary(x => x.LeaveType, x => x);
    }

    public async Task UpdateLeaveBalanceAsync(string employeeId, string leaveType, double daysUsed)
    {
        try
        {
            var balance = await _leaveBalanceRepo.GetBalanceAsync(employeeId, leaveType, DateTime.UtcNow.Year);
            if (balance != null)
            {
                balance.DaysUsed += daysUsed;
                balance.DaysRemaining = balance.TotalDaysAllowed - balance.DaysUsed;
                await _leaveBalanceRepo.UpdateBalanceAsync(balance);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating leave balance: {ex.Message}", ex);
            throw;
        }
    }
}
