using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Enrollments.Commands.CreateEnrollment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize]
public sealed class EnrollmentController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollClient(
        [FromBody] CreateEnrollmentDto dto,
        [FromServices] ICommandHandler<CreateEnrollmentCommand, int> commandHandler,
        CancellationToken cancellationToken)
    {
        var enrollment = await commandHandler.Handle(new CreateEnrollmentCommand(dto.ClientId, dto.ClassId), cancellationToken);
        return CreatedAtAction(nameof(EnrollClient), new { enrollmentId = enrollment }, enrollment);
    }
}
