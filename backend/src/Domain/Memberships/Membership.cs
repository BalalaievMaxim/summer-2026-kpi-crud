using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Domain.Memberships.Errors;

namespace GymManagement.Domain.Memberships;

public sealed class Membership : AggregateRoot<Guid>
{
    public Guid ClientId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Membership() { }

    private Membership(Guid id, Guid clientId, Guid planId, DateRange period)
        : base(id)
    {
        ClientId = clientId;
        PlanId = planId;
        Period = period;
        IsActive = true;
    }

    public static Membership Create(Guid clientId, Guid planId, DateOnly startDate, int durationMonths)
    {
        if (clientId == Guid.Empty)
            throw new InvalidMembershipError("ClientId cannot be empty.");
        if (planId == Guid.Empty)
            throw new InvalidMembershipError("PlanId cannot be empty.");

        var period = DateRange.CreateFromMonths(startDate, durationMonths);
        return new Membership(Guid.NewGuid(), clientId, planId, period);
    }

    public bool IsCurrentlyActive() => IsActive && Period.IsActive();

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
