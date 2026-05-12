namespace GymManagement.Application.DTOs;

public sealed record MembershipDto(
    int MembershipId,
    int ClientId,
    int PlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive);

public sealed record MembershipPlanDto(
    int PlanId,
    string Name,
    int DurationMonths,
    decimal Price);
