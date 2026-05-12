using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Coaches.Queries.GetCoachesBySpecialization;

public sealed class GetCoachesBySpecializationQueryHandler(ICoachReadRepository coachReadRepository)
    : IQueryHandler<GetCoachesBySpecializationQuery, IReadOnlyList<CoachSummaryDto>>
{
    public async Task<IReadOnlyList<CoachSummaryDto>> Handle(
        GetCoachesBySpecializationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await coachReadRepository.GetBySpecializationAsync(query.Specialization, cancellationToken);
    }
}