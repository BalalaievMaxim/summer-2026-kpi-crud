using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Coaches;

namespace GymManagement.Application.Features.Coaches.Queries.GetAllCoaches;

public sealed class GetAllCoachesQueryHandler(ICoachReadRepository coachReadRepository)
    : IQueryHandler<GetAllCoachesQuery, IReadOnlyList<CoachSummaryDto>>
{
    public async Task<IReadOnlyList<CoachSummaryDto>> Handle(
        GetAllCoachesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await coachReadRepository.GetAllSummaryAsync(cancellationToken);
    }
}
