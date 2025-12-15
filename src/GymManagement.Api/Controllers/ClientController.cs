using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Core.Entities;
using GymManagement.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/v1/clients")]
public class ClientController(ClientService clientService) : ControllerBase
{
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
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Email is already in use"))
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
        }
    }
    
    // hard delete
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
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
        }
    }

    // пошук клієнтів за ім'ям або email (простий запит)
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<Client>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchClients([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { error = "Search term cannot be empty." });
        }
        var clients = await clientService.SearchClientsAsync(searchTerm);
        return Ok(clients);
    }

    // історія занять клієнта (простий запит)
    [HttpGet("{clientId}/history")]
    [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientHistory(int clientId)
    {
        try
        {
            var client = await clientService.GetClientClassHistoryAsync(clientId);
            return Ok(client);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
        }
    }
    
    // аналітика активності клієнтів (складний запит з віконною функцією)
    [HttpGet("analytics/activity")]
    [ProducesResponseType(typeof(List<ClientActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClientActivityDto>>> GetClientActivityAnalytics()
    {
        var analytics = await clientService.GetClientActivityAnalyticsAsync();
        return Ok(analytics);
    }
}