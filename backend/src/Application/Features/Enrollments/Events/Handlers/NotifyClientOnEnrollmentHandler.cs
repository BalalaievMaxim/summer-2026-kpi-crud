using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;

namespace GymManagement.Application.Features.Enrollments.Events.Handlers;

public sealed class NotifyClientOnEnrollmentHandler(
    IClientRepository clientRepository,
    INotificationService notificationService) : IEventHandler<EnrollmentCreatedEvent>
{
    public async Task HandleAsync(EnrollmentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(@event.ClientId, cancellationToken);
        if (client is null) return;

        await notificationService.SendEmailAsync(
            client.Email.Value,
            "Class Enrollment Confirmation",
            $"You have successfully enrolled in class {@event.ClassId}. Time: {@event.RegistrationTime}",
            cancellationToken);
    }
}