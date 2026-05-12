using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Invoices.Queries.GetMonthlyRevenueByPlan;

public sealed class GetMonthlyRevenueByPlanQueryHandler(IInvoiceAnalyticsRepository invoiceAnalyticsRepository)
    : IQueryHandler<GetMonthlyRevenueByPlanQuery, List<TotalMembershipRevenueRow>>
{
    public Task<List<TotalMembershipRevenueRow>> Handle(
        GetMonthlyRevenueByPlanQuery query,
        CancellationToken cancellationToken = default)
        => invoiceAnalyticsRepository.GetMonthlyRevenueByPlanAsync(cancellationToken);
}
