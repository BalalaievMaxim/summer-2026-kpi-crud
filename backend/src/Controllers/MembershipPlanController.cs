using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymManagement.Application.Services;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("/api/v1/membership-plans")]
public class MembershipPlanController(IMembershipPlanService membershipPlanService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePlan([FromBody] CreateMembershipPlanDto dto)
    {
        try
        {
            await membershipPlanService.CreatePlanAsync(dto);
            return StatusCode(201, "Membership plan created successfully.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
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
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<MembershipPlan>>> GetPlans([FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        try
        {
            var plans = await membershipPlanService.GetPlansAsync(minPrice, maxPrice);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPlan>> GetPlan(int id)
    {
        try
        {
            var plan = await membershipPlanService.GetPlanByIdAsync(id);
            
            if (plan == null)
            {
                return NotFound($"Plan with ID {id} not found.");
            }
            
            return Ok(plan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
