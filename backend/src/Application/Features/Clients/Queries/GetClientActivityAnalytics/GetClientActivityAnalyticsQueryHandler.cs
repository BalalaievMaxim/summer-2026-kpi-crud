using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Clients.Queries.GetClientActivityAnalytics;

public sealed class GetClientActivityAnalyticsQueryHandler(IClientAnalyticsRepository clientAnalyticsRepository)
    : IQueryHandler<GetClientActivityAnalyticsQuery, List<ClientActivityRow>>
{
    public Task<List<ClientActivityRow>> Handle(
        GetClientActivityAnalyticsQuery query,
        CancellationToken cancellationToken = default)
        => clientAnalyticsRepository.GetClientActivityAnalyticsAsync(cancellationToken);
}
