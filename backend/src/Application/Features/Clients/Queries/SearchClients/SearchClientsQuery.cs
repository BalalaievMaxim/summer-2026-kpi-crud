using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;

namespace GymManagement.Application.Features.Clients.Queries.SearchClients;

public sealed record SearchClientsQuery(string SearchTerm) : IQuery<IReadOnlyList<ClientSummaryDto>>;
