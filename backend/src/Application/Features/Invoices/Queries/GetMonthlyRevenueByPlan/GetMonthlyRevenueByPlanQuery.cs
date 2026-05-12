using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Invoices.Queries.GetMonthlyRevenueByPlan;

public sealed record GetMonthlyRevenueByPlanQuery : IQuery<List<TotalMembershipRevenueRow>>;
