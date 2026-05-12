using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Clients.Commands.RegisterClient;

public sealed class RegisterClientCommandHandler(
    IClientRepository clientRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<RegisterClientCommand, int>
{
    public async Task<int> Handle(RegisterClientCommand command, CancellationToken cancellationToken = default)
    {
        if (await clientRepository.ExistsByEmailAsync(command.Email, cancellationToken: cancellationToken))
            throw new ClientEmailAlreadyExistsError(command.Email);

        var client = Client.Create(command.Name, command.Email, command.Phone, command.Password, passwordHasher);
        return await clientRepository.AddAsync(client, cancellationToken);
    }
}
