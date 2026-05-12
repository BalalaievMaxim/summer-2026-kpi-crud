using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;

public sealed class GetMembershipPlanByIdQueryHandler(IMembershipPlanReadRepository membershipPlanReadRepository)
    : IQueryHandler<GetMembershipPlanByIdQuery, MembershipPlanDto?>
{
    public async Task<MembershipPlanDto?> Handle(GetMembershipPlanByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await membershipPlanReadRepository.GetByIdAsync(query.PlanId, cancellationToken);
    }
}