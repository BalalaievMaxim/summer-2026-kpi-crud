using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/v1/memberships")]
public class MembershipController(
    IMembershipService membershipService,
    IMembershipRepository membershipRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateMembership([FromBody] PurchaseMembershipDto dto)
    {
        try
        {
            var method = dto.PaymentMethod;

            await membershipService.PurchaseMembershipAsync(
                dto.ClientId,
                dto.PlanId,
                method,
                dto.Notes);

            var result = await membershipRepository.GetActiveMembershipsByClientAsync(dto.ClientId);

            return CreatedAtAction(nameof(GetActiveByClient), new { clientId = dto.ClientId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { error = $"Database Error: {innerMessage}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active/{clientId}")]
    public async Task<IActionResult> GetActiveByClient(int clientId)
    {
        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        return Ok(memberships);
    }
}