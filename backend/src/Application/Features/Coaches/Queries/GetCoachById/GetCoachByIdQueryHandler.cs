using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Coaches.Queries.GetCoachById;

public sealed class GetCoachByIdQueryHandler(ICoachReadRepository coachReadRepository)
    : IQueryHandler<GetCoachByIdQuery, CoachDto?>
{
    public async Task<CoachDto?> Handle(GetCoachByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await coachReadRepository.GetByIdAsync(query.CoachId, cancellationToken);
    }
}