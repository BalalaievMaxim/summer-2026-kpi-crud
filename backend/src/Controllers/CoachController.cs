using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.Commands.DeleteCoach;
using GymManagement.Application.Features.Coaches.Commands.RegisterCoach;
using GymManagement.Application.Features.Coaches.Commands.UpdateCoachSpecialization;
using GymManagement.Application.Features.Coaches.Queries.GetAllCoaches;
using GymManagement.Application.Features.Coaches.Queries.GetCoachById;
using GymManagement.Application.Features.Coaches.Queries.GetCoachesBySpecialization;
using GymManagement.Application.Features.Coaches.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoachController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetAllCoachesQuery, IReadOnlyList<CoachSummaryDto>> queryHandler,
        CancellationToken cancellationToken)
    {
        var coaches = await queryHandler.Handle(new GetAllCoachesQuery(), cancellationToken);
        return Ok(coaches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IQueryHandler<GetCoachByIdQuery, CoachDto?> queryHandler,
        CancellationToken cancellationToken)
    {
        var coach = await queryHandler.Handle(new GetCoachByIdQuery(id), cancellationToken);
        if (coach is null) return NotFound();
        return Ok(coach);
    }

    [HttpGet("specialization/{specialization}")]
    public async Task<IActionResult> GetBySpecialization(
        string specialization,
        [FromServices] IQueryHandler<GetCoachesBySpecializationQuery, IReadOnlyList<CoachSummaryDto>> queryHandler,
        CancellationToken cancellationToken)
    {
        var coaches = await queryHandler.Handle(new GetCoachesBySpecializationQuery(specialization), cancellationToken);
        return Ok(coaches);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] CreateCoachDto dto,
        [FromServices] ICommandHandler<RegisterCoachCommand, int> commandHandler,
        [FromServices] IQueryHandler<GetCoachByIdQuery, CoachDto?> queryHandler,
        CancellationToken cancellationToken)
    {
        var coachId = await commandHandler.Handle(
            new RegisterCoachCommand(dto.Name, dto.Email, dto.Specialization, dto.Password),
            cancellationToken);

        var coach = await queryHandler.Handle(new GetCoachByIdQuery(coachId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = coachId }, coach);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ICommandHandler<DeleteCoachCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new DeleteCoachCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/specialization")]
    public async Task<IActionResult> UpdateSpecialization(
        int id,
        [FromBody] string specialization,
        [FromServices] ICommandHandler<UpdateCoachSpecializationCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new UpdateCoachSpecializationCommand(id, specialization), cancellationToken);
        return NoContent();
    }
}
