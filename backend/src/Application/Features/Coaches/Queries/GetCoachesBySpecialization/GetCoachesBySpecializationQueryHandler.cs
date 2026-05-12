using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Domain.Coaches;

namespace GymManagement.Application.Features.Coaches.Queries.GetCoachesBySpecialization;

public sealed class GetCoachesBySpecializationQueryHandler(ICoachRepository coachRepository)
    : IQueryHandler<GetCoachesBySpecializationQuery, IReadOnlyList<CoachSummaryDto>>
{
    public async Task<IReadOnlyList<CoachSummaryDto>> Handle(
        GetCoachesBySpecializationQuery query,
        CancellationToken cancellationToken = default)
    {
        var coaches = await coachRepository.GetBySpecializationAsync(query.Specialization, cancellationToken);
        return coaches.Select(CoachMappings.ToSummaryDto).ToList();
    }
}
