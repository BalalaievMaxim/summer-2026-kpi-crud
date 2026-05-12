using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Auth.Queries.LoginClient;

public sealed class LoginClientQueryHandler(
    IClientRepository clientRepository,
    IPasswordHasher passwordHasher) : IQueryHandler<LoginClientQuery, ClientDto>
{
    public async Task<ClientDto> Handle(LoginClientQuery query, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByEmailAsync(query.Email, cancellationToken)
            ?? throw new InvalidCredentialsError();

        if (!client.MatchesPassword(query.Password, passwordHasher))
            throw new InvalidCredentialsError();

        return ClientMappings.ToDto(client);
    }
}
