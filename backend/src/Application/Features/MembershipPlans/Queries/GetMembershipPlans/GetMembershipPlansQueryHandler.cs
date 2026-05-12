using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public sealed class GetMembershipPlansQueryHandler(IMembershipPlanReadRepository membershipPlanReadRepository)
    : IQueryHandler<GetMembershipPlansQuery, List<MembershipPlanDto>>
{
    public async Task<List<MembershipPlanDto>> Handle(GetMembershipPlansQuery query, CancellationToken cancellationToken = default)
    {
        return await membershipPlanReadRepository.GetPlansAsync(query.MinPrice, query.MaxPrice, cancellationToken);
    }
}