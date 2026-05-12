using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Domain.Memberships.Errors;

namespace GymManagement.Domain.Memberships;

public sealed class Membership : AggregateRoot<int>
{
    public int ClientId { get; private set; }
    public int PlanId { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Membership() { }

    private Membership(int id, int clientId, int planId, DateRange period, bool isActive)
        : base(id)
    {
        ClientId = clientId;
        PlanId = planId;
        Period = period;
        IsActive = isActive;
    }

    public static Membership PurchasePending(int clientId, MembershipPlan plan, DateOnly startDate)
    {
        if (clientId <= 0)
            throw new InvalidMembershipError("ClientId must be a positive number.");
        if (plan.Id <= 0)
            throw new InvalidMembershipError("PlanId must be a positive number.");

        var period = DateRange.CreateFromMonths(startDate, plan.DurationMonths);
        return new Membership(0, clientId, plan.Id, period, isActive: false);
    }

    public static Membership Reconstitute(int id, int clientId, int planId, DateOnly startDate, DateOnly endDate, bool isActive)
        => new(id, clientId, planId, DateRange.Create(startDate, endDate), isActive);

    public bool IsCurrentlyActive(DateOnly today) => IsActive && Period.IsActive(today);

    public bool IsForPlan(int planId) => PlanId == planId;

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new MembershipAlreadyInactiveError("Membership is already inactive.");
        IsActive = false;
    }

    public void Renew(int months)
    {
        if (months <= 0)
            throw new InvalidMembershipError("Renew period be positive.");
        var newEnd = Period.End.AddMonths(months);
        Period = DateRange.Create(Period.Start, newEnd);
        IsActive = true;
    }
}
