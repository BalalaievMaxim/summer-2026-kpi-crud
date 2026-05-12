using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("/api/v1/membership-plans")]
[Authorize]
public sealed class MembershipPlanController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePlan(
        [FromBody] CreateMembershipPlanDto dto,
        [FromServices] ICommandHandler<CreateMembershipPlanCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new CreateMembershipPlanCommand(dto.Name, dto.DurationMonth, dto.Price), cancellationToken);
        return StatusCode(201, "Membership plan created successfully.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlan(
        int id,
        [FromServices] ICommandHandler<DeleteMembershipPlanCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new DeleteMembershipPlanCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<MembershipPlanDto>>> GetPlans(
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromServices] IQueryHandler<GetMembershipPlansQuery, List<MembershipPlanDto>> queryHandler,
        CancellationToken cancellationToken)
    {
        var plans = await queryHandler.Handle(new GetMembershipPlansQuery(minPrice, maxPrice), cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPlanDto>> GetPlan(
        int id,
        [FromServices] IQueryHandler<GetMembershipPlanByIdQuery, MembershipPlanDto?> queryHandler,
        CancellationToken cancellationToken)
    {
        var plan = await queryHandler.Handle(new GetMembershipPlanByIdQuery(id), cancellationToken);

        if (plan is null)
            return NotFound($"Plan with ID {id} not found.");

        return Ok(plan);
    }
}
