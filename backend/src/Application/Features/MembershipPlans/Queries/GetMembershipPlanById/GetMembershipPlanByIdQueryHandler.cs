using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;

public sealed class GetMembershipPlanByIdQueryHandler(IMembershipPlanRepositoryPort membershipPlanRepository)
    : IQueryHandler<GetMembershipPlanByIdQuery, MembershipPlanDto?>
{
    public async Task<MembershipPlanDto?> Handle(GetMembershipPlanByIdQuery query, CancellationToken cancellationToken = default)
    {
        var plan = await membershipPlanRepository.GetByIdAsync(query.PlanId, cancellationToken);
        return plan is null ? null : ToDto(plan);
    }

    private static MembershipPlanDto ToDto(MembershipPlan plan)
        => new(plan.Id, plan.Name, plan.DurationMonths, plan.Price.Amount);
}
