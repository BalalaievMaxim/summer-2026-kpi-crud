using DomainMembership = GymManagement.Domain.Memberships.Membership;
using MembershipEntity = GymManagement.Infrastructure.Persistence.Entities.Membership;

namespace GymManagement.Infrastructure.Persistence.Mappers;

public static class MembershipMapper
{
    public static DomainMembership ToDomain(MembershipEntity e)
    {
        var clientId = IntToGuid(e.ClientId);
        var planId = IntToGuid(e.PlanId);
        var months = MonthsBetween(e.StartDate, e.EndDate);

        return DomainMembership.Create(clientId, planId, e.StartDate, months);
    }

    public static MembershipEntity ToEntity(DomainMembership d)
    {
        return new MembershipEntity
        {
            MembershipId = GuidToInt(d.Id),
            ClientId = GuidToInt(d.ClientId),
            PlanId = GuidToInt(d.PlanId),
            StartDate = d.Period.Start,
            EndDate = d.Period.End,
            IsActive = d.IsActive
        };
    }

    private static Guid IntToGuid(int id) => new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    private static int MonthsBetween(DateOnly start, DateOnly end)
        => (end.Year - start.Year) * 12 + end.Month - start.Month;
}
