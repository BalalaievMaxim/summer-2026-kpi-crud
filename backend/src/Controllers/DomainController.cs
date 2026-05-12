using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Features.Classes.Queries.GetScheduleForDate;
using GymManagement.Application.Features.Coaches.Queries.GetAllCoaches;
using GymManagement.Application.Features.Coaches.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/test-domain2")]
[Authorize]
public sealed class DomainController : ControllerBase
{
    [HttpGet("coaches")]
    public async Task<IActionResult> GetAllCoaches(
        [FromServices] IQueryHandler<GetAllCoachesQuery, IReadOnlyList<CoachSummaryDto>> queryHandler,
        CancellationToken cancellationToken)
    {
        var coaches = await queryHandler.Handle(new GetAllCoachesQuery(), cancellationToken);
        return Ok(coaches);
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
