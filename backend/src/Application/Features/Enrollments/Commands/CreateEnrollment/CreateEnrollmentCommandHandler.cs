using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
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
    EnrollmentFactory enrollmentFactory) : ICommandHandler<CreateEnrollmentCommand, int>
{
    public async Task<int> Handle(CreateEnrollmentCommand command, CancellationToken cancellationToken = default)
    {
        if (!await clientRepository.ExistsAsync(command.ClientId, cancellationToken))
            throw new ClientNotFoundError(command.ClientId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeMemberships = await membershipRepository.GetActiveMembershipsByClientAsync(command.ClientId, cancellationToken);
        var hasActiveMembership = activeMemberships.Any(m => m.IsCurrentlyActive(today));

        if (!hasActiveMembership)
            throw new ClientHasNoActiveMembershipError(command.ClientId);

        var enrollment = await enrollmentFactory.CreateAsync(command.ClientId, command.ClassId, DateTimeOffset.UtcNow, cancellationToken);
        return await enrollmentRepository.AddAsync(enrollment, cancellationToken);
}
    }

