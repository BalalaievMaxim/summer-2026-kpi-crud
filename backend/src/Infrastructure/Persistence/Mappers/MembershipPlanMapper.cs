using DomainMembershipPlan = GymManagement.Domain.Memberships.MembershipPlan;
using MembershipPlanEntity = GymManagement.Infrastructure.Persistence.Entities.MembershipPlan;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Infrastructure.Persistence.Mappers;

public static class MembershipPlanMapper
{
    public static DomainMembershipPlan ToDomain(MembershipPlanEntity e)
    {
        var price = Money.Create(e.Price, "UAH");
        var zones = e.PlanAccesses.Select(pa => IntToGuid(pa.ZoneId));

        return DomainMembershipPlan.Create(e.Name, e.DurationMonths, price, zones);
    }

    public static MembershipPlanEntity ToEntity(DomainMembershipPlan d)
    {
        return new MembershipPlanEntity
        {
            PlanId = GuidToInt(d.Id),
            Name = d.Name,
            DurationMonths = d.DurationMonths,
            Price = d.Price.Amount
        };
    }

    private static Guid IntToGuid(int id) => new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
