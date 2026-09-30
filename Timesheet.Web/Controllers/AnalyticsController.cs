namespace Timesheet.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Domain.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAnalytics(DateTime startDate, DateTime endDate)
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            await _analyticsService.GenerateTimesheetAnalyticsAsync(employeeId, startDate, endDate);
            return Ok(new { message = "Analytics generated" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating analytics: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("insights")]
    public async Task<IActionResult> GetDashboardInsights()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var insights = await _analyticsService.GetDashboardInsightsAsync(employeeId);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting insights: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("employee")]
    public async Task<IActionResult> GetEmployeeAnalytics()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var analytics = await _analyticsService.GetEmployeeAnalyticsAsync(employeeId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("anomalies")]
    public async Task<IActionResult> GetAnomalies()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            var anomalies = await _analyticsService.GetAnomaliesAsync(employeeId);
            return Ok(anomalies);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("detect-anomalies")]
    public async Task<IActionResult> DetectAnomalies()
    {
        try
        {
            var employeeId = User.FindFirst("sub")?.Value;
            await _analyticsService.DetectAnomaliesAsync(employeeId);
            return Ok(new { message = "Anomalies detected" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
