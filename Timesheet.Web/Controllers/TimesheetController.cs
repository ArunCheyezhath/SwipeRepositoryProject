namespace Timesheet.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimesheetController : ControllerBase
{
    private readonly ITimesheetCalculationService _calculationService;
    private readonly IFileImportService _importService;
    private readonly IEmployeeProvider _employeeProvider;

    public TimesheetController(ITimesheetCalculationService calc, IFileImportService import, IEmployeeProvider emp)
    {
        _calculationService = calc;
        _importService = import;
        _employeeProvider = emp;
    }

    [HttpPost("import/employees")]
    public async Task<IActionResult> ImportEmployees(IFormFile file, string employeeId)
    {
        if (file?.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        var result = await _importService.ImportEmployeesAsync(stream, employeeId);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("import/swipes")]
    public async Task<IActionResult> ImportSwipes(IFormFile file, string employeeId)
    {
        if (file?.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        var result = await _importService.ImportSwipesAsync(stream, employeeId);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("import/leaves")]
    public async Task<IActionResult> ImportLeaves(IFormFile file, string employeeId)
    {
        if (file?.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        var result = await _importService.ImportLeavesAsync(stream, employeeId);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("import/holidays")]
    public async Task<IActionResult> ImportHolidays(IFormFile file)
    {
        if (file?.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        var result = await _importService.ImportHolidaysAsync(stream);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sync/employees")]
    public async Task<IActionResult> SyncEmployees(string employeeId)
    {
        var result = await _importService.SyncEmployeeFromApiAsync(employeeId);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sync/swipes")]
    public async Task<IActionResult> SyncSwipes(string employeeId)
    {
        var result = await _importService.SyncSwipesFromApiAsync(employeeId);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sync/leaves")]
    public async Task<IActionResult> SyncLeaves(string employeeId)
    {
        var result = await _importService.SyncLeavesFromApiAsync(employeeId);
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sync/holidays")]
    public async Task<IActionResult> SyncHolidays()
    {
        var result = await _importService.SyncHolidaysFromApiAsync();
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(string employeeId, DateTime startDate, DateTime endDate)
    {
        var timesheet = await _calculationService.GenerateTimesheetAsync(employeeId, startDate, endDate);
        return Ok(timesheet);
    }

    [HttpPost("recalculate")]
    public async Task<IActionResult> Recalculate(string employeeId, DateTime startDate, DateTime endDate)
    {
        var entries = await _calculationService.RecalculateAsync(employeeId, startDate, endDate);
        return Ok(entries);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] TimesheetSummary timesheet)
    {
        return Ok(new { message = "Timesheet submitted successfully", timestamp = DateTime.UtcNow });
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _employeeProvider.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
