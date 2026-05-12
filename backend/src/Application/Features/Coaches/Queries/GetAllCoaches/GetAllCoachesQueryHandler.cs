using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Domain.Coaches;

namespace GymManagement.Application.Features.Coaches.Queries.GetAllCoaches;

public sealed class GetAllCoachesQueryHandler(ICoachRepository coachRepository)
    : IQueryHandler<GetAllCoachesQuery, IReadOnlyList<CoachSummaryDto>>
{
    public async Task<IReadOnlyList<CoachSummaryDto>> Handle(
        GetAllCoachesQuery query,
        CancellationToken cancellationToken = default)
    {
        var coaches = await coachRepository.GetAllAsync(cancellationToken);
        return coaches.Select(CoachMappings.ToSummaryDto).ToList();
    }
}
