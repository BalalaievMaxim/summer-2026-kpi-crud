using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;

public sealed record GetMembershipPlanByIdQuery(int PlanId) : IQuery<MembershipPlanDto?>;
