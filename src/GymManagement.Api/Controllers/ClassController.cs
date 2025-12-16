using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassController : ControllerBase
{
    private readonly IClassRepository _classRepository;
    private readonly ICoachRepository _coachRepository;
    private readonly IClassTypeRepository _classTypeRepository;

    public ClassController(
        IClassRepository classRepository,
        ICoachRepository coachRepository,
        IClassTypeRepository classTypeRepository)
    {
        _classRepository = classRepository;
        _coachRepository = coachRepository;
        _classTypeRepository = classTypeRepository;
    }

    [HttpPost]
    public async Task<ActionResult<Class>> Create([FromBody] CreateClassRequest request)
    {
        try
        {
            if (request.EndTime <= request.StartTime)
                return BadRequest("End time must be after start time");

            if (request.Capacity <= 0 || request.Capacity > 10)
                return BadRequest("Capacity must be between 1 and 10");

            if (!await _classTypeRepository.ExistsAsync(request.ClassTypeId))
                return BadRequest($"ClassType {request.ClassTypeId} not found");

            if (!await _coachRepository.ExistsAsync(request.CoachId))
                return BadRequest($"Coach {request.CoachId} not found");

            var hasConflict = await _classRepository.HasTimeConflictForCoachAsync(
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

            var created = await _classRepository.CreateAsync(newClass);
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
        var classEntity = await _classRepository.GetByIdAsync(id);
        if (classEntity == null)
            return NotFound();
        return Ok(classEntity);
    }

    [HttpPut("{id}/reschedule")]
    public async Task<ActionResult<Class>> Reschedule(int id, [FromBody] RescheduleRequest request)
    {
        try
        {
            var existing = await _classRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (existing.StartTime <= DateTime.UtcNow)
                return BadRequest("Cannot reschedule a class that has already started");

            if (request.NewEndTime <= request.NewStartTime)
                return BadRequest("End time must be after start time");

            var hasConflict = await _classRepository.HasTimeConflictForCoachAsync(
                existing.CoachId, request.NewStartTime, request.NewEndTime, id);

            if (hasConflict)
                return BadRequest("Coach already has another class scheduled during this time");

            existing.StartTime = request.NewStartTime;
            existing.EndTime = request.NewEndTime;

            var updated = await _classRepository.UpdateAsync(existing);
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
        var schedule = await _classRepository.GetScheduleForDateAsync(date);
        return Ok(schedule);
    }

    [HttpGet("analytics/coach-efficiency")]
    public async Task<ActionResult<List<CoachEfficiencyDto>>> GetCoachEfficiency(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        var allCoaches = await _coachRepository.GetAllAsync();
        var results = new List<CoachEfficiencyDto>();

        foreach (var coach in allCoaches)
        {
            var classes = (await _classRepository.GetClassesByCoachAsync(
                coach.CoachId, startDate, endDate)).ToList();

            if (classes.Any())
            {
                var totalHours = (int)classes.Sum(c => (c.EndTime - c.StartTime).TotalHours);
                var avgOccupancy = classes.Average(c => 
                    c.Capacity > 0 ? (double)c.Enrollments.Count / c.Capacity * 100 : 0);

                results.Add(new CoachEfficiencyDto
                {
                    CoachId = coach.CoachId,
                    CoachName = coach.Name,
                    Specialization = coach.Specialization,
                    TotalHours = totalHours,
                    ClassCount = classes.Count,
                    AverageOccupancyPercent = Math.Round(avgOccupancy, 1)
                });
            }
        }

        return Ok(results);
    }
}

