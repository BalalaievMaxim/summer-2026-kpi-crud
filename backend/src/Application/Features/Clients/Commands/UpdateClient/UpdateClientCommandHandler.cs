using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Clients.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateClientCommand>
{
    public async Task Handle(UpdateClientCommand command, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(command.ClientId, cancellationToken)
            ?? throw new ClientNotFoundError(command.ClientId);

        if (!string.Equals(client.Email.Value, command.Email, StringComparison.OrdinalIgnoreCase)
            && await clientRepository.ExistsByEmailAsync(command.Email, command.ClientId, cancellationToken))
        {
            throw new ClientEmailAlreadyExistsError(command.Email);
        }

        client.UpdateContact(command.Email, command.Phone);

        await clientRepository.UpdateAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
