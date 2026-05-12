using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Memberships.Commands.PurchaseMembership;
using GymManagement.Application.Features.Memberships.Queries.GetActiveMembershipsByClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/v1/memberships")]
[Authorize]
public sealed class MembershipController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateMembership(
        [FromBody] PurchaseMembershipDto dto,
        [FromServices] ICommandHandler<PurchaseMembershipCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(
            new PurchaseMembershipCommand(
                dto.ClientId,
                dto.PlanId,
                dto.PaymentMethod,
                dto.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetActiveByClient), new { clientId = dto.ClientId }, null);
    }

    [HttpGet("active/{clientId}")]
    public async Task<IActionResult> GetActiveByClient(
        int clientId,
        [FromServices] IQueryHandler<GetActiveMembershipsByClientQuery, IReadOnlyList<MembershipDto>> queryHandler,
        CancellationToken cancellationToken)
    {
        var memberships = await queryHandler.Handle(new GetActiveMembershipsByClientQuery(clientId), cancellationToken);
        return Ok(memberships);
    }
}
