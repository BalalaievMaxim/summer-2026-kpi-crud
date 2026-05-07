using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoachController(ICoachService coachService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var coach = await coachService.GetByIdAsync(id);
        if (coach is null) return NotFound();
        return Ok(new { id = coach.Id, name = coach.Name.Value, email = coach.Email.Value, specialization = coach.Specialization.Value });
    }

    [HttpGet("specialization/{specialization}")]
    public async Task<IActionResult> GetBySpecialization(string specialization)
    {
        var coaches = await coachService.GetBySpecializationAsync(specialization);
        return Ok(coaches.Select(c => new { id = c.Id, name = c.Name.Value, specialization = c.Specialization.Value }));
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CreateCoachDto dto)
    {
        try
        {
            var coach = await coachService.RegisterCoachAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = coach.Id }, new
            {
                id = coach.Id,
                name = coach.Name.Value,
                email = coach.Email.Value,
                specialization = coach.Specialization.Value
            });
        }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await coachService.DeleteCoachAsync(id);
            return NoContent();
        }
        catch (CoachNotFoundError ex) { return NotFound(new { code = ex.Code, error = ex.Message }); }
        catch (CoachHasFutureClassesError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }

    [HttpPatch("{id}/specialization")]
    public async Task<IActionResult> UpdateSpecialization(int id, [FromBody] string specialization)
    {
        try
        {
            await coachService.UpdateSpecializationAsync(id, specialization);
            return NoContent();
        }
        catch (CoachNotFoundError ex) { return NotFound(new { code = ex.Code, error = ex.Message }); }
        catch (DomainError ex) { return BadRequest(new { code = ex.Code, error = ex.Message }); }
    }
}
