using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Domain.Memberships.Errors;

namespace GymManagement.Domain.Memberships;

public sealed class MembershipPlan : AggregateRoot<int>
{
    private readonly List<int> _accessZones = new();

    public string Name { get; private set; } = string.Empty;
    public int DurationMonths { get; private set; }
    public Money Price { get; private set; } = null!;
    public IReadOnlyList<int> AccessZones => _accessZones.AsReadOnly();

    private MembershipPlan() { }

    private MembershipPlan(int id, string name, int durationMonths, Money price, IEnumerable<int> accessZones)
    {
        Id = id;
        Name = name;
        DurationMonths = durationMonths;
        Price = price;
        _accessZones.AddRange(accessZones);
    }

    public static MembershipPlan Create(string name, int durationMonths, Money price, IEnumerable<int>? accessZones = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidMembershipPlanError("Plan name cannot be empty.");
        if (durationMonths <= 0)
            throw new InvalidMembershipPlanError("Duration must be at least 1 month.");
        if (price.Amount <= 0)
            throw new InvalidMembershipPlanError("Price must be greater than zero.");

        return new MembershipPlan(0, name.Trim(), durationMonths, price, accessZones ?? Enumerable.Empty<int>());
    }

    public static MembershipPlan Create(string name, int durationMonths, decimal price, IEnumerable<int>? accessZones = null)
    {
        if (price <= 0)
            throw new InvalidMembershipPlanError("Price must be greater than zero.");

        return Create(name, durationMonths, Money.Create(price), accessZones);
    }

    public static MembershipPlan Reconstitute(int id, string name, int durationMonths, decimal price, IEnumerable<int>? accessZones = null)
        => new(id, name, durationMonths, Money.Create(price), accessZones ?? Enumerable.Empty<int>());

    public bool IncludesZone(int zoneId) => _accessZones.Contains(zoneId);

    public void AddAccessZone(int zoneId)
    {
        if (!_accessZones.Contains(zoneId))
            _accessZones.Add(zoneId);
    }

    public void UpdatePrice(Money newPrice) => Price = newPrice;
}
