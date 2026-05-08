using GymManagement.Domain.Memberships;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Mappers;

public static class MembershipPlanMapper
{
    public static MembershipPlan ToDomain(MembershipPlanEntity e)
    {
        var price = Money.Create(e.Price, e.Currency);
        var zones = e.PlanAccesses.Select(pa => IntToGuid(pa.ZoneId));

        return MembershipPlan.Create(e.Name, e.DurationMonths, price, zones);
    }

    public static MembershipPlanEntity ToEntity(MembershipPlan d)
    {
        return new MembershipPlanEntity
        {
            PlanId = GuidToInt(d.Id),
            Name = d.Name,
            DurationMonths = d.DurationMonths,
            Price = d.Price.Amount,
            Currency = d.Price.Currency
        };
    }

    private static Guid IntToGuid(int id) => new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
