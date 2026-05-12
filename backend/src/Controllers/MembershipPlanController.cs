using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("/api/v1/membership-plans")]
[Authorize]
public sealed class MembershipPlanController(IMembershipPlanService membershipPlanService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePlan([FromBody] CreateMembershipPlanDto dto)
    {
        try
        {
            await membershipPlanService.CreatePlanAsync(dto);
            return StatusCode(201, "Membership plan created successfully.");
        }
        catch (DomainError ex)
        {
            return BadRequest(new { code = ex.Code, error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Unable to create membership plan." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        try
        {
            await membershipPlanService.DeleteUnusedPlanAsync(id);
            return NoContent();
        }
        catch (DomainError ex)
        {
            return ex.Code.EndsWith(".NotFound", StringComparison.Ordinal)
                ? NotFound(new { code = ex.Code, error = ex.Message })
                : Conflict(new { code = ex.Code, error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Unable to delete membership plan." });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<MembershipPlanDto>>> GetPlans([FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        try
        {
            var plans = await membershipPlanService.GetPlansAsync(minPrice, maxPrice);
            return Ok(plans);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Unable to load membership plans." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPlanDto>> GetPlan(int id)
    {
        try
        {
            var plan = await membershipPlanService.GetPlanByIdAsync(id);

            if (plan is null)
                return NotFound($"Plan with ID {id} not found.");

            return Ok(plan);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Unable to load membership plan." });
        }
    }
}
