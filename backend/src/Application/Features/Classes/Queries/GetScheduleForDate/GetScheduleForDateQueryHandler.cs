using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Classes.Queries.GetScheduleForDate;

public sealed class GetScheduleForDateQueryHandler(IClassScheduleRepository classScheduleRepository)
    : IQueryHandler<GetScheduleForDateQuery, IReadOnlyList<GymClassDetails>>
{
    public Task<IReadOnlyList<GymClassDetails>> Handle(GetScheduleForDateQuery query, CancellationToken cancellationToken = default)
        => classScheduleRepository.GetScheduleForDateAsync(query.Date, cancellationToken);
}
