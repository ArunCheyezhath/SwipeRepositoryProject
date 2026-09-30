namespace Timesheet.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveManagementController : ControllerBase
{
    private readonly ILeaveManagementService _leaveService;
    private readonly ILogger<LeaveManagementController> _logger;

    public LeaveManagementController(ILeaveManagementService leaveService, ILogger<LeaveManagementController> logger)
    {
        _leaveService = leaveService;
        _logger = logger;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestLeave([FromBody] LeaveRequestDto dto)
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var result = await _leaveService.RequestLeaveAsync(employeeId, dto.StartDate, dto.EndDate, dto.LeaveType, dto.Reason);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error requesting leave: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetMyLeaveRequests()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var results = await _leaveService.GetEmployeeLeaveRequestsAsync(employeeId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetLeaveBalance(string leaveType)
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var balance = await _leaveService.GetLeaveBalanceAsync(employeeId, leaveType);
            return Ok(balance);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetAllBalances()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var balances = await _leaveService.GetAllBalancesAsync(employeeId);
            return Ok(balances);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{requestId}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> ApproveLeave(string requestId)
    {
        try
        {
            var approverId = User.FindFirst("sub")?.Value;
            await _leaveService.ApproveLeaveAsync(requestId, approverId);
            return Ok(new { message = "Leave approved" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{requestId}/reject")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> RejectLeave(string requestId, [FromBody] RejectLeaveDto dto)
    {
        try
        {
            var approverId = User.FindFirst("sub")?.Value;
            await _leaveService.RejectLeaveAsync(requestId, approverId, dto.Reason);
            return Ok(new { message = "Leave rejected" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class LeaveRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; }
    public string Reason { get; set; }
}

public class RejectLeaveDto
{
    public string Reason { get; set; }
}
