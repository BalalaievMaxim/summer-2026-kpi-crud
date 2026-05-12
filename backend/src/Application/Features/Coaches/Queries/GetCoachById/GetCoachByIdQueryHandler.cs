using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Domain.Coaches;

namespace GymManagement.Application.Features.Coaches.Queries.GetCoachById;

public sealed class GetCoachByIdQueryHandler(ICoachRepository coachRepository)
    : IQueryHandler<GetCoachByIdQuery, CoachDto?>
{
    public async Task<CoachDto?> Handle(GetCoachByIdQuery query, CancellationToken cancellationToken = default)
    {
        var coach = await coachRepository.GetByIdAsync(query.CoachId, cancellationToken);
        return coach is null ? null : CoachMappings.ToDto(coach);
    }
}
