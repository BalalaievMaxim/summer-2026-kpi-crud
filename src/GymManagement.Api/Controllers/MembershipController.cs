using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipController(MembershipService membershipService) : ControllerBase
{
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseMembership([FromBody] PurchaseMembershipDto dto)
    {
        try
        {
            await membershipService.PurchaseMembershipAsync(dto.ClientId, dto.PlanId, dto.PaymentMethod, dto.Notes);
            return Ok("Membership purchased and invoice created successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}