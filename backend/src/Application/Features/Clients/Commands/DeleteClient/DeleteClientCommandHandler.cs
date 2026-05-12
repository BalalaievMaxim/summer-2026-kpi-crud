using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Clients.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteClientCommand>
{
    public async Task Handle(DeleteClientCommand command, CancellationToken cancellationToken = default)
    {
        if (!await clientRepository.ExistsAsync(command.ClientId, cancellationToken))
            throw new ClientNotFoundError(command.ClientId);

        if (await clientRepository.HasActiveEnrollmentsAsync(command.ClientId, cancellationToken))
            throw new ClientHasActiveEnrollmentsError(command.ClientId);

        if (await clientRepository.HasActiveMembershipsAsync(command.ClientId, cancellationToken))
            throw new ClientHasActiveMembershipsError(command.ClientId);

        await clientRepository.DeleteAsync(command.ClientId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
