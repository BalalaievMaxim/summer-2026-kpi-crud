using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public sealed class GetMembershipPlansQueryHandler(IMembershipPlanRepositoryPort membershipPlanRepository)
    : IQueryHandler<GetMembershipPlansQuery, List<MembershipPlanDto>>
{
    public async Task<List<MembershipPlanDto>> Handle(GetMembershipPlansQuery query, CancellationToken cancellationToken = default)
    {
        var plans = await membershipPlanRepository.GetPlansAsync(query.MinPrice, query.MaxPrice, cancellationToken);
        return plans.Select(ToDto).ToList();
    }

    private static MembershipPlanDto ToDto(MembershipPlan plan)
        => new(plan.Id, plan.Name, plan.DurationMonths, plan.Price.Amount);
}
