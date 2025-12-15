using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpPost]
    public async Task<ActionResult<Class>> CreateClass([FromBody] CreateClassRequest request)
    {
        var newClass = await _classService.CreateClassAsync(
            request.ClassTypeId,
            request.CoachId,
            request.StartTime,
            request.EndTime,
            request.Capacity
        );

        return CreatedAtAction(nameof(GetScheduleForDate), new { date = newClass.StartTime.Date }, newClass);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteClass(int id)
    {
        var result = await _classService.DeleteClassAsync(id);
        if (!result)
            return NotFound($"Class with ID {id} not found.");

        return NoContent();
    }

    [HttpGet("schedule/{date}")]
    public async Task<ActionResult<IEnumerable<Class>>> GetScheduleForDate(DateTime date)
    {
        var schedule = await _classService.GetScheduleForDateAsync(date);
        return Ok(schedule);
    }

    [HttpGet("schedule/week")]
    public async Task<ActionResult<IEnumerable<Class>>> GetScheduleForWeek([FromQuery] DateTime startDate)
    {
        var schedule = await _classService.GetScheduleForWeekAsync(startDate);
        return Ok(schedule);
    }

    [HttpGet("analytics/attendance")]
    public async Task<ActionResult<IEnumerable<ClassAttendanceDto>>> GetAttendanceAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var analytics = await _classService.GetClassAttendanceAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("analytics/coach/{coachId}")]
    public async Task<ActionResult<CoachWorkloadDto>> GetCoachWorkload(
        int coachId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var workload = await _classService.GetCoachWorkloadAsync(coachId, startDate, endDate);
        return Ok(workload);
    }
}

public record CreateClassRequest(int ClassTypeId, int CoachId, DateTime StartTime, DateTime EndTime, int Capacity);
