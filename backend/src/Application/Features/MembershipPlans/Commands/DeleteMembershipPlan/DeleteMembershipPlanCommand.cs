using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;

public sealed record DeleteMembershipPlanCommand(int PlanId) : ICommand;
