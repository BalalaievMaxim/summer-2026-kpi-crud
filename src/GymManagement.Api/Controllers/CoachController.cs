using System.Collections.Generic;
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

    public CoachController(ICoachRepository coachRepository)
    {
        _coachRepository = coachRepository;
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
        var result = await _coachRepository.DeleteAsync(id);
        if (!result)
            return NotFound($"Coach with ID {id} not found.");

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
