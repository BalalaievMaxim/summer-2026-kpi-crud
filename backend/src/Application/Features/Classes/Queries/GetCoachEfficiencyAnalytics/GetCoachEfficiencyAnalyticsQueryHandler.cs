using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Classes.Queries.GetCoachEfficiencyAnalytics;

public sealed class GetCoachEfficiencyAnalyticsQueryHandler(IClassScheduleRepository classScheduleRepository)
    : IQueryHandler<GetCoachEfficiencyAnalyticsQuery, List<CoachEfficiencyRow>>
{
    public Task<List<CoachEfficiencyRow>> Handle(
        GetCoachEfficiencyAnalyticsQuery query,
        CancellationToken cancellationToken = default)
        => classScheduleRepository.GetCoachEfficiencyAnalyticsAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);
}
