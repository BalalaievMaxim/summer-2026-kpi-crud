using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Features.Classes.Queries.GetScheduleForDate;
using GymManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/test-domain2")]
[Authorize]
public sealed class DomainController(ICoachService coachService) : ControllerBase
{
    [HttpGet("coaches")]
    public async Task<IActionResult> GetAllCoaches()
    {
        var coaches = await coachService.GetAllAsync();
        return Ok(coaches.Select(c => new { id = c.Id, name = c.Name.Value, specialization = c.Specialization.Value }));
    }

    [HttpGet("schedule/{date}")]
    public async Task<IActionResult> GetSchedule(
        DateTime date,
        [FromServices] IQueryHandler<GetScheduleForDateQuery, IReadOnlyList<GymClassDetails>> queryHandler,
        CancellationToken cancellationToken)
    {
        var schedule = await queryHandler.Handle(new GetScheduleForDateQuery(date), cancellationToken);
        return Ok(schedule);
    }
}
