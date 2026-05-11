using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/v1/memberships")]
[Authorize]
public sealed class MembershipController(IMembershipService membershipService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateMembership([FromBody] PurchaseMembershipDto dto)
    {
        try
        {
            await membershipService.PurchaseMembershipAsync(
                dto.ClientId,
                dto.PlanId,
                dto.PaymentMethod,
                dto.Notes);

            var result = await membershipService.GetActiveMembershipsByClientAsync(dto.ClientId);

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
        catch (Exception)
        {
            return StatusCode(500, new { error = "Unable to process membership purchase." });
        }
    }

    [HttpGet("active/{clientId}")]
    public async Task<IActionResult> GetActiveByClient(int clientId)
    {
        var memberships = await membershipService.GetActiveMembershipsByClientAsync(clientId);
        return Ok(memberships);
    }
}
