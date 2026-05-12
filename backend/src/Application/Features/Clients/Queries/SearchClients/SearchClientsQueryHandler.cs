using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Shared;

namespace GymManagement.Application.Features.Clients.Queries.SearchClients;

public sealed class SearchClientsQueryHandler(IClientRepository clientRepository)
    : IQueryHandler<SearchClientsQuery, IReadOnlyList<ClientSummaryDto>>
{
    public async Task<IReadOnlyList<ClientSummaryDto>> Handle(SearchClientsQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.SearchTerm))
            throw new DomainValidationError("Client.EmptySearchTerm", "Search term cannot be empty.");

        var clients = await clientRepository.SearchAsync(query.SearchTerm, cancellationToken);
        return clients.Select(ClientMappings.ToSummaryDto).ToList();
    }
}
