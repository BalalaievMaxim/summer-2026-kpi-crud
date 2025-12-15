using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoachController : ControllerBase
{
    private readonly ICoachRepository _coachRepository;
    private readonly IClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CoachController(
        ICoachRepository coachRepository, 
        IClassRepository classRepository,
        IUnitOfWork unitOfWork)
    {
        _coachRepository = coachRepository;
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Coach>>> GetAll()
    {
        var coaches = await _coachRepository.GetAllAsync();
        return Ok(coaches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Coach>> GetById(int id)
    {
        var coach = await _coachRepository.GetByIdAsync(id);
        if (coach == null)
            return NotFound($"Coach with ID {id} not found.");
        
        return Ok(coach);
    }

    [HttpPost]
    public async Task<ActionResult<Coach>> Create([FromBody] CreateCoachRequest request)
    {
        var coach = new Coach
        {
            Name = request.Name,
            Specialization = request.Specialization,
            Email = request.Email,
            Password = request.Password
        };

        var created = await _coachRepository.CreateAsync(coach);
        return CreatedAtAction(nameof(GetById), new { id = created.CoachId }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Coach>> Update(int id, [FromBody] UpdateCoachRequest request)
    {
        var coach = await _coachRepository.GetByIdAsync(id);
        if (coach == null)
            return NotFound($"Coach with ID {id} not found.");

        coach.Name = request.Name;
        coach.Specialization = request.Specialization;
        coach.Email = request.Email;

        var updated = await _coachRepository.UpdateAsync(coach);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var coach = await _coachRepository.GetByIdAsync(id);
        if (coach == null)
            return NotFound($"Coach with ID {id} not found.");

        // CASCADE DELETE: Видалити всі майбутні класи тренера
        var futureClasses = await _classRepository.GetUpcomingClassesByCoachAsync(id);
        
        foreach (var classEntity in futureClasses)
        {
            // Перевірка чи є записані клієнти
            if (classEntity.Enrollments.Count > 0)
            {
                return BadRequest(
                    $"Cannot delete coach. Class '{classEntity.ClassType.Name}' on {classEntity.StartTime:yyyy-MM-dd HH:mm} has {classEntity.Enrollments.Count} enrolled client(s). Please cancel enrollments first.");
            }
            
            await _classRepository.DeleteAsync(classEntity.ClassId);
        }

        // Видалити тренера
        await _coachRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/classes")]
    public async Task<ActionResult<IEnumerable<Class>>> GetCoachClasses(int id)
    {
        var coach = await _coachRepository.GetByIdAsync(id);
        if (coach == null)
            return NotFound($"Coach with ID {id} not found.");

        return Ok(coach.Classes);
    }

    [HttpGet("specialization/{specialization}")]
    public async Task<ActionResult<IEnumerable<Coach>>> GetBySpecialization(string specialization)
    {
        var coaches = await _coachRepository.GetBySpecializationAsync(specialization);
        return Ok(coaches);
    }
}

public record CreateCoachRequest(string Name, string Specialization, string Email, string Password);
public record UpdateCoachRequest(string Name, string Specialization, string Email);
