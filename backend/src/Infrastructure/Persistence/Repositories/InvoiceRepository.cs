using GymManagement.Domain.Billing;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Queries;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(GymManagementContext context) : IInvoiceRepositoryPort
{
    public async Task<List<InvoiceRecord>> GetAllClientInvoicesAsync(int clientId,
        CancellationToken cancellationToken = default)
    {
        var list = await context.Invoices
            .Where(i => i.ClientId == clientId)
            .ToListAsync(cancellationToken);

        return list.Select(ToRecord).ToList();
    }

    public async Task<List<InvoiceRecord>> GetPendingInvoicesAsync(int clientId,
        CancellationToken cancellationToken = default)
    {
        var list = await context.Invoices
            .Where(i => i.ClientId == clientId && i.Status == "pending")
            .ToListAsync(cancellationToken);

        return list.Select(ToRecord).ToList();
    }

    public async Task<InvoiceRecord?> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var entity = await context.Invoices.FindAsync([invoiceId], cancellationToken);
        return entity is null ? null : ToRecord(entity);
    }

    public async Task AddAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default)
    {
        var entity = new E.Invoice
        {
            ClientId = invoice.ClientId,
            Amount = invoice.Amount,
            Date = invoice.Date,
            Status = invoice.Status,
            PaymentMethod = invoice.PaymentMethod,
            Notes = invoice.Notes
        };

        await context.Invoices.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateStatusAsync(int invoiceId, string status, CancellationToken cancellationToken = default)
    {
        await context.Invoices
            .Where(i => i.InvoiceId == invoiceId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, status), cancellationToken);
    }

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

    private static InvoiceRecord ToRecord(E.Invoice i) =>
        new(i.InvoiceId, i.ClientId, i.Amount, i.Date, i.Status, i.PaymentMethod ?? string.Empty, i.Notes);
}
