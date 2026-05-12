using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Auth.Commands.LoginClient;

public sealed class LoginClientCommandHandler(
    IClientRepository clientRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<LoginClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(LoginClientCommand command, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByEmailAsync(command.Email, cancellationToken)
            ?? throw new InvalidCredentialsError();

        if (!client.MatchesPassword(command.Password, passwordHasher))
            throw new InvalidCredentialsError();

        return ClientMappings.ToDto(client);
    }
}
