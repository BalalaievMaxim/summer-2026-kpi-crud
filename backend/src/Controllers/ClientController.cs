using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Shared;
using GymManagement.Infrastructure.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/v1/clients")]
public class ClientController(IClientService clientService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] CreateClientDto dto)
    {
        try
        {
            var client = await clientService.RegisterClientAsync(dto);
            return CreatedAtAction(nameof(Register), new { id = client.Id }, new
            {
                id = client.Id,
                name = client.Name.Value,
                email = client.Email.Value,
                phone = client.Phone.Value
            });
        }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var client = await clientService.LoginClientAsync(dto.Email, dto.Password);
            return Ok(new { id = client.Id, name = client.Name.Value, email = client.Email.Value });
        }
        catch (InvalidCredentialsError ex) { return Unauthorized(new { code = ex.Code, error = ex.Message }); }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }

    [HttpPut("{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClient(int clientId, [FromBody] UpdateClientDto dto)
    {
        try
        {
            await clientService.UpdateClientAsync(clientId, dto);
            return NoContent();
        }
        catch (ClientNotFoundError ex) { return NotFound(new { code = ex.Code, error = ex.Message }); }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }

    [HttpDelete("{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClient(int clientId)
    {
        try
        {
            await clientService.DeleteClientAsync(clientId);
            return NoContent();
        }
        catch (ClientNotFoundError ex) { return NotFound(new { code = ex.Code, error = ex.Message }); }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchClients([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest(new { error = "Search term cannot be empty." });

        var clients = await clientService.SearchClientsAsync(searchTerm);
        return Ok(clients.Select(c => new { clientId = c.Id, name = c.Name.Value, email = c.Email.Value }));
    }

    [HttpGet("{clientId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientHistory(int clientId)
    {
        var client = await clientService.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        return Ok(new { id = client.Id, name = client.Name.Value, email = client.Email.Value, phone = client.Phone.Value });
    }

    [HttpGet("analytics/activity")]
    [ProducesResponseType(typeof(List<ClientActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClientActivityDto>>> GetClientActivityAnalytics()
    {
        var analytics = await clientService.GetClientActivityAnalyticsAsync();
        return Ok(analytics);
    }
}
