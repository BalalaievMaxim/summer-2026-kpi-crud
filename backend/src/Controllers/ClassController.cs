using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassController(
    IClassRepository classRepository,
    ICoachService coachService,
    IClassTypeRepository classTypeRepository) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Class>> Create([FromBody] CreateClassRequest request)
    {
        try
        {
            if (request.EndTime <= request.StartTime)
                return BadRequest("End time must be after start time");

            if (request.Capacity <= 0 || request.Capacity > 10)
                return BadRequest("Capacity must be between 1 and 10");

            if (!await classTypeRepository.ExistsAsync(request.ClassTypeId))
                return BadRequest($"ClassType {request.ClassTypeId} not found");

            var coach = await coachService.GetByIdAsync(request.CoachId);
            if (coach is null)
                return BadRequest($"Coach {request.CoachId} not found");

            var hasConflict = await classRepository.HasTimeConflictForCoachAsync(
                request.CoachId, request.StartTime, request.EndTime);

            if (hasConflict)
                return BadRequest("Coach already has a class scheduled during this time");

            var newClass = new Class
            {
                ClassTypeId = request.ClassTypeId,
                CoachId = request.CoachId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Capacity = request.Capacity
            };

            var created = await classRepository.CreateAsync(newClass);
            return CreatedAtAction(nameof(GetById), new { id = created.ClassId }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Class>> GetById(int id)
    {
        var classEntity = await classRepository.GetByIdAsync(id);
        if (classEntity == null)
            return NotFound();
        return Ok(classEntity);
    }

    [HttpPut("{id}/reschedule")]
    public async Task<ActionResult<Class>> Reschedule(int id, [FromBody] RescheduleRequest request)
    {
        try
        {
            var existing = await classRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (existing.StartTime <= DateTime.UtcNow)
                return BadRequest("Cannot reschedule a class that has already started");

            if (request.NewEndTime <= request.NewStartTime)
                return BadRequest("End time must be after start time");

            var hasConflict = await classRepository.HasTimeConflictForCoachAsync(
                existing.CoachId, request.NewStartTime, request.NewEndTime, id);

            if (hasConflict)
                return BadRequest("Coach already has another class scheduled during this time");

            existing.StartTime = request.NewStartTime;
            existing.EndTime = request.NewEndTime;

            var updated = await classRepository.UpdateAsync(existing);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("schedule/{date}")]
    public async Task<ActionResult<List<Class>>> GetSchedule(DateTime date)
    {
        var schedule = await classRepository.GetScheduleForDateAsync(date);
        return Ok(schedule);
    }

    [HttpGet("analytics/coach-efficiency")]
    public async Task<ActionResult<List<CoachEfficiencyDto>>> GetCoachEfficiency(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var allCoaches = await coachService.GetAllAsync();
        var results = new List<CoachEfficiencyDto>();

        foreach (var coach in allCoaches)
        {
            var classes = (await classRepository.GetClassesByCoachAsync(coach.Id, startDate, endDate)).ToList();
            if (classes.Any())
            {
                var totalHours = (int)classes.Sum(c => (c.EndTime - c.StartTime).TotalHours);
                var avgOccupancy = classes.Average(c =>
                    c.Capacity > 0 ? (double)c.Enrollments.Count / c.Capacity * 100 : 0);

                results.Add(new CoachEfficiencyDto
                {
                    CoachId = coach.Id,
                    CoachName = coach.Name.Value,
                    Specialization = coach.Specialization.Value,
                    TotalHours = totalHours,
                    ClassCount = classes.Count,
                    AverageOccupancyPercent = Math.Round(avgOccupancy, 1)
                });
            }
        }

        return Ok(results);
    }
}
