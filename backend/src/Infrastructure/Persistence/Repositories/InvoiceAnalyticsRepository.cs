using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class InvoiceAnalyticsRepository(GymManagementContext context) : IInvoiceAnalyticsRepository
{
    public async Task<List<TotalMembershipRevenueRow>> GetMonthlyRevenueByPlanAsync(
        CancellationToken cancellationToken = default)
    {
        var raw = await context.Invoices
            .Where(i => i.Status == "paid")
            .Join(context.Memberships,
                i => i.ClientId,
                m => m.ClientId,
                (i, m) => new { Invoice = i, Membership = m })
            .Join(context.Membershipplans,
                im => im.Membership.PlanId,
                p => p.PlanId,
                (im, p) => new { im.Invoice, Plan = p })
            .GroupBy(x => new { x.Invoice.Date.Month, x.Plan.Name })
            .Select(g => new
            {
                Month = g.Key.Month,
                PlanName = g.Key.Name,
                TotalRevenue = g.Sum(x => x.Invoice.Amount)
            })
            .ToListAsync(cancellationToken);

        return raw
            .Select(x => new TotalMembershipRevenueRow(x.Month.ToString(), x.PlanName, x.TotalRevenue))
            .ToList();
    }
}
