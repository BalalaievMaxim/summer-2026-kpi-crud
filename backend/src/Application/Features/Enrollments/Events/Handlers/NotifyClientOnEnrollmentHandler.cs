using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Enrollments.Events.Handlers;

public sealed class NotifyClientOnEnrollmentHandler(INotificationService notificationService) : IEventHandler<EnrollmentCreatedEvent>
{
    public async Task HandleAsync(EnrollmentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await notificationService.SendEmailAsync(
            @event.ClientEmail,
            "Class Enrollment Confirmation",
            $"Hello {@event.ClientName}, you have successfully enrolled in class {@event.ClassId}. Time: {@event.RegistrationTime}",
            cancellationToken);
    }
}