using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Classes.Commands.CreateClass;
using GymManagement.Application.Features.Classes.Commands.DeleteClass;
using GymManagement.Application.Features.Classes.Commands.RescheduleClass;
using GymManagement.Application.Features.Classes.Queries.GetClassById;
using GymManagement.Application.Features.Classes.Queries.GetCoachEfficiencyAnalytics;
using GymManagement.Application.Features.Classes.Queries.GetScheduleForDate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClassController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassRequest request,
        [FromServices] ICommandHandler<CreateClassCommand, int> commandHandler,
        [FromServices] IQueryHandler<GetClassByIdQuery, GymClassDetails?> queryHandler,
        CancellationToken cancellationToken)
    {
        var classId = await commandHandler.Handle(
            new CreateClassCommand(
                request.ClassTypeId,
                request.CoachId,
                request.StartTime,
                request.EndTime,
                request.Capacity),
            cancellationToken);

        var created = await queryHandler.Handle(new GetClassByIdQuery(classId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = classId }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IQueryHandler<GetClassByIdQuery, GymClassDetails?> queryHandler,
        CancellationToken cancellationToken)
    {
        var classEntity = await queryHandler.Handle(new GetClassByIdQuery(id), cancellationToken);
        if (classEntity is null)
            return NotFound();
        return Ok(classEntity);
    }

    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(
        int id,
        [FromBody] RescheduleRequest request,
        [FromServices] ICommandHandler<RescheduleClassCommand> commandHandler,
        [FromServices] IQueryHandler<GetClassByIdQuery, GymClassDetails?> queryHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new RescheduleClassCommand(id, request.NewStartTime, request.NewEndTime), cancellationToken);

        var updated = await queryHandler.Handle(new GetClassByIdQuery(id), cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ICommandHandler<DeleteClassCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new DeleteClassCommand(id), cancellationToken);
        return NoContent();
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

    [HttpGet("analytics/coach-efficiency")]
    public async Task<ActionResult<List<CoachEfficiencyRow>>> GetCoachEfficiency(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromServices] IQueryHandler<GetCoachEfficiencyAnalyticsQuery, List<CoachEfficiencyRow>> queryHandler,
        CancellationToken cancellationToken)
    {
        var results = await queryHandler.Handle(new GetCoachEfficiencyAnalyticsQuery(startDate, endDate), cancellationToken);
        return Ok(results);
    }
}
