namespace GymManagement.Infrastructure.Persistence.Entities;

public class MembershipEntity
{
    public int MembershipId { get; set; }
    public int ClientId { get; set; }
    public int PlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ClientEntity Client { get; set; } = null!;
    public MembershipPlanEntity MembershipPlan { get; set; } = null!;
}
