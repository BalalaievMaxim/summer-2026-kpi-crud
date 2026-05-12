using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Domain.Clients;

namespace GymManagement.Application.Features.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IClientRepository clientRepository)
    : IQueryHandler<GetClientByIdQuery, ClientDto?>
{
    public async Task<ClientDto?> Handle(GetClientByIdQuery query, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        return client is null ? null : ClientMappings.ToDto(client);
    }
}
