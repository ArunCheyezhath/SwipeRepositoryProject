namespace Timesheet.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Domain.Interfaces;
using Timesheet.Domain.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsProvider _settingsProvider;

    public SettingsController(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _settingsProvider.GetSettingsAsync());
    }

    [HttpPut("{dataType}")]
    public async Task<IActionResult> Update(string dataType, [FromBody] DataSourceSettings settings)
    {
        try
        {
            await _settingsProvider.UpdateSettingsAsync(dataType, settings);
            return Ok(await _settingsProvider.GetSettingsAsync());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
