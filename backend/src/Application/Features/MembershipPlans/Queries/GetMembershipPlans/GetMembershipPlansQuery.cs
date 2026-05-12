using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public sealed record GetMembershipPlansQuery(decimal? MinPrice, decimal? MaxPrice) : IQuery<List<MembershipPlanDto>>;
