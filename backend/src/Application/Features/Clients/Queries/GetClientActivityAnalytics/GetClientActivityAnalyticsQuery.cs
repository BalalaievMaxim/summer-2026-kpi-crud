using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Clients.Queries.GetClientActivityAnalytics;

public sealed record GetClientActivityAnalyticsQuery : IQuery<List<ClientActivityRow>>;
