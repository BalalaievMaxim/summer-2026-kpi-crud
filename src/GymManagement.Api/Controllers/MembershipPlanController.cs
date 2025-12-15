using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/v1/membership-plans")]
public class MembershipPlanController(
    MembershipPlanService membershipPlanService,
    IMembershipPlanRepository planRepository) : ControllerBase
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
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
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
            return Conflict(new { error = ex.Message }); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
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
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlanById(int id)
    {
        var plan = await planRepository.GetMembershipPlanByIdAsync(id);
        if (plan == null) return NotFound();
        return Ok(plan);
    }
}