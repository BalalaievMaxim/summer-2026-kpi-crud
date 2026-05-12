using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Shared;

namespace GymManagement.Application.Features.Clients.Queries.SearchClients;

public sealed class SearchClientsQueryHandler(IClientReadRepository clientReadRepository)
    : IQueryHandler<SearchClientsQuery, IReadOnlyList<ClientSummaryDto>>
{
    public async Task<IReadOnlyList<ClientSummaryDto>> Handle(SearchClientsQuery query, CancellationToken cancellationToken = default)
    {
        return await clientReadRepository.SearchAsync(query.SearchTerm, cancellationToken);
    }
}
