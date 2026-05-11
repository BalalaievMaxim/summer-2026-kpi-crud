namespace GymManagement.Domain.Memberships;

public sealed record MembershipPlanSnapshot(
    int PlanId,
    string Name,
    int DurationMonths,
    decimal Price);

public sealed record MembershipRecord(
    int MembershipId,
    int ClientId,
    int PlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive);
