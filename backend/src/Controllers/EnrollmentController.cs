using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Application.Exceptions;
using GymManagement.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize]
public sealed class EnrollmentController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResultDto), StatusCodes.Status201Created)]
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
        catch (DomainError ex)
        {
            return BadRequest(new { code = ex.Code, error = ex.Message });
        }
    }
}
