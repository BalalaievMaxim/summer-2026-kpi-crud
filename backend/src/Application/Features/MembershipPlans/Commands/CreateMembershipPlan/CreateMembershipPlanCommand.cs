using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public sealed record CreateMembershipPlanCommand(
    string Name,
    int DurationMonths,
    decimal Price) : ICommand;
