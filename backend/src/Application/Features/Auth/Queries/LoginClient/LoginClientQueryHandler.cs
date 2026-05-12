using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Auth.Queries.LoginClient;

public sealed class LoginClientQueryHandler(
    IClientRepository clientRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IQueryHandler<LoginClientQuery, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(LoginClientQuery query, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByEmailAsync(query.Email, cancellationToken)
            ?? throw new InvalidCredentialsError();

        if (!client.MatchesPassword(query.Password, passwordHasher))
            throw new InvalidCredentialsError();

        // Application шар вирішує, як створити сесію
        var token = tokenService.CreateToken(client.Id, client.Email.Value, "Client");

        return new AuthResultDto(
            client.Id,
            client.Name.Value,
            client.Email.Value,
            client.Phone.Value,
            token
        );
    }
}
