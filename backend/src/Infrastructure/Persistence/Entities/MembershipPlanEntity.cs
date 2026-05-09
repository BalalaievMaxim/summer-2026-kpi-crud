namespace GymManagement.Infrastructure.Persistence.Entities;

public class MembershipPlanEntity
{
    public int PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "UAH";

    public ICollection<PlanAccess> PlanAccesses { get; set; } = new List<PlanAccess>();
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
