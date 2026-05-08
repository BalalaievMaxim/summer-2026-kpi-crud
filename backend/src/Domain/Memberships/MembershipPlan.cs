using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Domain.Memberships.Errors;

namespace GymManagement.Domain.Memberships;

public sealed class MembershipPlan : AggregateRoot<Guid>
{
    private readonly List<Guid> _accessZones = new();

    public string Name { get; private set; } = string.Empty;
    public int DurationMonths { get; private set; }
    public Money Price { get; private set; } = null!;
    public IReadOnlyList<Guid> AccessZones => _accessZones.AsReadOnly();

    private MembershipPlan() { }

    private MembershipPlan(Guid id, string name, int durationMonths, Money price, IEnumerable<Guid> accessZones)
        : base(id)
    {
        Name = name;
        DurationMonths = durationMonths;
        Price = price;
        _accessZones.AddRange(accessZones);
    }

    public static MembershipPlan Create(string name, int durationMonths, Money price, IEnumerable<Guid>? accessZones = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidMembershipPlanError("Plan name cannot be empty.");
        if (durationMonths <= 0)
            throw new InvalidMembershipPlanError("Duration must be at least 1 month.");

        return new MembershipPlan(Guid.NewGuid(), name, durationMonths, price, accessZones ?? Enumerable.Empty<Guid>());
    }

    public bool IncludesZone(Guid zoneId) => _accessZones.Contains(zoneId);

    public void AddAccessZone(Guid zoneId)
    {
        if (!_accessZones.Contains(zoneId))
            _accessZones.Add(zoneId);
    }

    public void UpdatePrice(Money newPrice) => Price = newPrice;
}
