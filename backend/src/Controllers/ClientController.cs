using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Auth.Commands.LoginClient;
using GymManagement.Application.Features.Clients.Commands.DeleteClient;
using GymManagement.Application.Features.Clients.Commands.RegisterClient;
using GymManagement.Application.Features.Clients.Commands.UpdateClient;
using GymManagement.Application.Features.Clients.Queries.GetClientActivityAnalytics;
using GymManagement.Application.Features.Clients.Queries.GetClientById;
using GymManagement.Application.Features.Clients.Queries.SearchClients;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/v1/clients")]
public class ClientController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] CreateClientDto dto,
        [FromServices] ICommandHandler<RegisterClientCommand, int> commandHandler,
        [FromServices] IQueryHandler<GetClientByIdQuery, ClientDto?> queryHandler,
        CancellationToken cancellationToken)
    {
        var clientId = await commandHandler.Handle(
            new RegisterClientCommand(dto.Name, dto.Email, dto.Phone, dto.Password),
            cancellationToken);

        var client = await queryHandler.Handle(new GetClientByIdQuery(clientId), cancellationToken);
        var token = tokenService.CreateToken(clientId, client!.Email, "Client");

        return CreatedAtAction(nameof(Register), new { id = clientId }, new
        {
            clientId = client.ClientId,
            client.Name,
            client.Email,
            client.Phone,
            token
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        [FromServices] ICommandHandler<LoginClientCommand, ClientDto> commandHandler,
        CancellationToken cancellationToken)
    {
        var client = await commandHandler.Handle(new LoginClientCommand(dto.Email, dto.Password), cancellationToken);
        var token = tokenService.CreateToken(client.ClientId, client.Email, "Client");
        return Ok(new { clientId = client.ClientId, client.Name, client.Email, client.Phone, token });
    }

    [HttpPut("{clientId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClient(
        int clientId,
        [FromBody] UpdateClientDto dto,
        [FromServices] ICommandHandler<UpdateClientCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new UpdateClientCommand(clientId, dto.Email, dto.Phone), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{clientId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClient(
        int clientId,
        [FromServices] ICommandHandler<DeleteClientCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new DeleteClientCommand(clientId), cancellationToken);
        return NoContent();
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchClients(
        [FromQuery] string searchTerm,
        [FromServices] IQueryHandler<SearchClientsQuery, IReadOnlyList<ClientSummaryDto>> queryHandler,
        CancellationToken cancellationToken)
    {
        var clients = await queryHandler.Handle(new SearchClientsQuery(searchTerm), cancellationToken);
        return Ok(clients);
    }

    [HttpGet("{clientId}/history")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientHistory(
        int clientId,
        [FromServices] IQueryHandler<GetClientByIdQuery, ClientDto?> queryHandler,
        CancellationToken cancellationToken)
    {
        var client = await queryHandler.Handle(new GetClientByIdQuery(clientId), cancellationToken);
        if (client is null) return NotFound();
        return Ok(client);
    }

    [HttpGet("analytics/activity")]
    [Authorize]
    [ProducesResponseType(typeof(List<ClientActivityRow>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClientActivityRow>>> GetClientActivityAnalytics(
        [FromServices] IQueryHandler<GetClientActivityAnalyticsQuery, List<ClientActivityRow>> queryHandler,
        CancellationToken cancellationToken)
    {
        var analytics = await queryHandler.Handle(new GetClientActivityAnalyticsQuery(), cancellationToken);
        return Ok(analytics);
    }
}
