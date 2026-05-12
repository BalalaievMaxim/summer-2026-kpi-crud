using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Classes.Queries.GetScheduleForWeek;

public sealed class GetScheduleForWeekQueryHandler(IClassScheduleRepository classScheduleRepository)
    : IQueryHandler<GetScheduleForWeekQuery, IReadOnlyList<GymClassDetails>>
{
    public Task<IReadOnlyList<GymClassDetails>> Handle(GetScheduleForWeekQuery query, CancellationToken cancellationToken = default)
        => classScheduleRepository.GetScheduleForDateRangeAsync(
            query.StartOfWeek,
            query.StartOfWeek.AddDays(7),
            cancellationToken);
}
