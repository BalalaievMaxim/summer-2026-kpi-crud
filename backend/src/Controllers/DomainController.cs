using System;
using System.Threading.Tasks;
using GymManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers;

[ApiController]
[Route("api/test-domain2")]
public class DomainController : ControllerBase
{
    private readonly ICoachRepository _coachRepo;
    private readonly IClassRepository _classRepo;

    public DomainController(ICoachRepository coachRepo, IClassRepository classRepo)
    {
        _coachRepo = coachRepo;
        _classRepo = classRepo;
    }

    [HttpGet("coaches")]
    public async Task<IActionResult> GetAllCoaches()
    {
        var coaches = await _coachRepo.GetAllAsync();
        return Ok(coaches);
    }

    [HttpGet("schedule/{date}")]
    public async Task<IActionResult> GetSchedule(DateTime date)
    {
        var schedule = await _classRepo.GetScheduleForDateAsync(date);
        return Ok(schedule);
    }
}
