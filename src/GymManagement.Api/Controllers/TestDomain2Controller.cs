using System;
using System.Threading.Tasks;
using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/test-domain2")]
public class TestDomain2Controller : ControllerBase
{
    private readonly ICoachRepository _coachRepo;
    private readonly IClassRepository _classRepo;

    public TestDomain2Controller(ICoachRepository coachRepo, IClassRepository classRepo)
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
