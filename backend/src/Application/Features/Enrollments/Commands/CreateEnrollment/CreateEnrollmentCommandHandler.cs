using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Features.Enrollments.Events;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Enrollments.Commands.CreateEnrollment;

public sealed class CreateEnrollmentCommandHandler(
    IEnrollmentRepositoryPort enrollmentRepository,
    IClientRepository clientRepository,
    IMembershipRepositoryPort membershipRepository,
    EnrollmentFactory enrollmentFactory,
    IEventBus eventBus) : ICommandHandler<CreateEnrollmentCommand, int>
{
    public async Task<int> Handle(CreateEnrollmentCommand command, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(command.ClientId, cancellationToken)
            ?? throw new ClientNotFoundError(command.ClientId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeMemberships = await membershipRepository.GetActiveMembershipsByClientAsync(command.ClientId, cancellationToken);
        var hasActiveMembership = activeMemberships.Any(m => m.IsCurrentlyActive(today));

        if (!hasActiveMembership)
            throw new ClientHasNoActiveMembershipError(command.ClientId);

        var enrollment = await enrollmentFactory.CreateAsync(command.ClientId, command.ClassId, DateTimeOffset.UtcNow, cancellationToken);

        var @event = new EnrollmentCreatedEvent(
            client.Id,
            client.Email.Value,
            client.Name.Value,
            command.ClassId,
            DateTime.UtcNow);

        await eventBus.PublishAsync(@event, cancellationToken);

        var id = await enrollmentRepository.AddAsync(enrollment, cancellationToken);

        return id;
    }
}