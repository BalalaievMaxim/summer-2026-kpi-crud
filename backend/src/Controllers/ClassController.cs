using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Queries;
using GymManagement.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClassController(IClassService classService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        try
        {
            if (request.EndTime <= request.StartTime)
                return BadRequest("End time must be after start time");

            if (request.Capacity <= 0 || request.Capacity > 10)
                return BadRequest("Capacity must be between 1 and 10");

            var created = await classService.CreateClassAsync(
                request.ClassTypeId,
                request.CoachId,
                request.StartTime,
                request.EndTime,
                request.Capacity);

            return CreatedAtAction(nameof(GetById), new { id = created.ClassId }, created);
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainError ex)
        {
            return BadRequest(new { code = ex.Code, error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var classEntity = await classService.GetClassByIdAsync(id);
        if (classEntity is null)
            return NotFound();
        return Ok(classEntity);
    }

    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequest request)
    {
        try
        {
            var updated = await classService.UpdateClassAsync(id, request.NewStartTime, request.NewEndTime);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("schedule/{date}")]
    public async Task<IActionResult> GetSchedule(DateTime date)
    {
        var schedule = await classService.GetScheduleForDateAsync(date);
        return Ok(schedule);
    }

    [HttpGet("analytics/coach-efficiency")]
    public async Task<ActionResult<List<CoachEfficiencyRow>>> GetCoachEfficiency(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var results = await classService.GetCoachEfficiencyAnalyticsAsync(startDate, endDate);
        return Ok(results);
    }
}
