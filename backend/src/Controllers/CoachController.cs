using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

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

    [HttpGet("{id}")]
    public async Task<ActionResult<Coach>> GetById(int id)
    {
        var coach = await _coachRepository.GetByIdAsync(id);
        if (coach == null)
            return NotFound();
        return Ok(coach);
    }

    [HttpGet("specialization/{specialization}")]
    public async Task<ActionResult<List<Coach>>> GetBySpecialization(string specialization)
    {
        var coaches = await _coachRepository.GetBySpecializationAsync(specialization);
        return Ok(coaches);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var coach = await _coachRepository.GetByIdAsync(id);
            if (coach == null)
                return NotFound();

            var futureClasses = (await _classRepository.GetUpcomingClassesByCoachAsync(id)).ToList();
            
            foreach (var classEntity in futureClasses)
            {
                var fullClass = await _classRepository.GetByIdAsync(classEntity.ClassId);
                
                if (fullClass != null && fullClass.Enrollments.Any())
                {
                    return BadRequest(
                        $"Cannot delete coach. Class '{fullClass.ClassType.Name}' on {fullClass.StartTime:yyyy-MM-dd HH:mm} has {fullClass.Enrollments.Count} enrolled client(s). Please cancel enrollments first.");
                }
            }

            foreach (var classEntity in futureClasses)
            {
                await _classRepository.DeleteAsync(classEntity.ClassId);
            }

            await _coachRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
