using GymManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/test-domain2")]
[Authorize]
public sealed class DomainController(ICoachService coachService, IClassService classService) : ControllerBase
{
    [HttpGet("coaches")]
    public async Task<IActionResult> GetAllCoaches()
    {
        var coaches = await coachService.GetAllAsync();
        return Ok(coaches.Select(c => new { id = c.Id, name = c.Name.Value, specialization = c.Specialization.Value }));
    }

    [HttpGet("schedule/{date}")]
    public async Task<IActionResult> GetSchedule(DateTime date)
    {
        var schedule = await classService.GetScheduleForDateAsync(date);
        return Ok(schedule);
    }
}
