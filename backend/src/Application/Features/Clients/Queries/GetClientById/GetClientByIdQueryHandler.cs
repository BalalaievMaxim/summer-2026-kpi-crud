using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IClientReadRepository clientReadRepository)
    : IQueryHandler<GetClientByIdQuery, ClientDto?>
{
    public async Task<ClientDto?> Handle(GetClientByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await clientReadRepository.GetByIdAsync(query.ClientId, cancellationToken);
    }
}