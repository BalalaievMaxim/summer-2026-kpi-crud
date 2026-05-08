using GymManagement.Domain.Memberships.Errors;

namespace GymManagement.Domain.Memberships;

public sealed class MembershipFactory
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMembershipPlanRepository _planRepository;

    public MembershipFactory(IMembershipRepository membershipRepository, IMembershipPlanRepository planRepository)
    {
        _membershipRepository = membershipRepository;
        _planRepository = planRepository;
    }

    public async Task<Membership> CreateAsync(Guid clientId, Guid planId, DateOnly startDate)
    {
        if (clientId == Guid.Empty)
            throw new InvalidMembershipError("ClientId cannot be empty.");
        if (planId == Guid.Empty)
            throw new InvalidMembershipError("PlanId cannot be empty.");
        if (startDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidMembershipError("Start date cannot be in the past.");

        var hasActive = await _membershipRepository.HasActiveMembershipAsync(clientId);
        if (hasActive)
            throw new ActiveMembershipExistsError(clientId);

        var plan = await _planRepository.GetByIdAsync(planId)
            ?? throw new MembershipPlanNotFoundError(planId);

        return Membership.Create(clientId, planId, startDate, plan.DurationMonths);
    }
}
