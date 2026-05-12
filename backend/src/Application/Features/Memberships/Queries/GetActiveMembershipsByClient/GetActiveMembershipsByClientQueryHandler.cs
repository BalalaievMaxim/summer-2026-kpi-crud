using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Memberships.Queries.GetActiveMembershipsByClient;

public sealed class GetActiveMembershipsByClientQueryHandler(IMembershipRepositoryPort membershipRepository)
    : IQueryHandler<GetActiveMembershipsByClientQuery, IReadOnlyList<MembershipDto>>
{
    public async Task<IReadOnlyList<MembershipDto>> Handle(
        GetActiveMembershipsByClientQuery query,
        CancellationToken cancellationToken = default)
    {
        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(query.ClientId, cancellationToken);
        return memberships.Select(ToDto).ToList();
    }

    private static MembershipDto ToDto(Membership membership)
        => new(
            membership.Id,
            membership.ClientId,
            membership.PlanId,
            membership.Period.Start,
            membership.Period.End,
            membership.IsActive);
}
