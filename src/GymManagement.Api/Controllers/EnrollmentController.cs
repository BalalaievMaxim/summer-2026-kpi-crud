using GymManagement.Core.DTOs;
using GymManagement.Application.Services;
using GymManagement.Core.Entities;
using GymManagement.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
public class EnrollmentController(EnrollmentService enrollmentService) : ControllerBase
{
    // запис клієнта на заняття (складний сценарій створення з перевірками)
    [HttpPost]
    [ProducesResponseType(typeof(Enrollment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollClient([FromBody] CreateEnrollmentDto dto)
    {
        try
        {
            var enrollment = await enrollmentService.CreateEnrollmentAsync(dto);
            return CreatedAtAction(nameof(EnrollClient), new { id = enrollment.EnrollmentId }, enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
        }
    }
}