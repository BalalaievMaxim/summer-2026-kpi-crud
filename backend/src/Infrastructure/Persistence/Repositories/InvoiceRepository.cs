using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(GymManagementContext context) : IInvoiceRepositoryPort, IInvoiceAnalyticsRepository
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

    public async Task<Invoice?> GetByIdAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var entity = await context.Invoices.FindAsync([invoiceId], cancellationToken);
        return entity is null ? null : ToAggregate(entity);
    }

    public async Task<int> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var entity = new E.Invoice
        {
            ClientId = invoice.ClientId,
            Amount = invoice.Amount,
            Date = invoice.Date,
            Status = ToDbStatus(invoice.Status),
            PaymentMethod = ToDbMethod(invoice.Method),
            Notes = invoice.Notes
        };

        await context.Invoices.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.InvoiceId;
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var entity = await context.Invoices.FindAsync([invoice.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Invoice {invoice.Id} not found in persistence.");

        entity.Status = ToDbStatus(invoice.Status);
        entity.PaymentMethod = ToDbMethod(invoice.Method);
        entity.Notes = invoice.Notes;
        entity.Amount = invoice.Amount;
        entity.Date = invoice.Date;
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

    private static Invoice ToAggregate(E.Invoice i) =>
        Invoice.Reconstitute(
            id: i.InvoiceId,
            clientId: i.ClientId,
            amount: i.Amount,
            date: i.Date,
            status: ParseStatus(i.Status),
            method: ParseMethod(i.PaymentMethod),
            notes: i.Notes);

    private static string ToDbStatus(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "pending",
        PaymentStatus.Paid => "paid",
        PaymentStatus.Overdue => "overdue",
        PaymentStatus.Cancelled => "cancelled",
        _ => "pending"
    };

    private static PaymentStatus ParseStatus(string raw) => raw?.ToLowerInvariant() switch
    {
        "paid" => PaymentStatus.Paid,
        "overdue" => PaymentStatus.Overdue,
        "cancelled" => PaymentStatus.Cancelled,
        _ => PaymentStatus.Pending
    };

    private static string ToDbMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "cash",
        PaymentMethod.Card => "card",
        PaymentMethod.BankTransfer => "bank_transfer",
        PaymentMethod.Online => "online",
        _ => "cash"
    };

    private static PaymentMethod ParseMethod(string? raw) => raw?.ToLowerInvariant() switch
    {
        "card" => PaymentMethod.Card,
        "bank_transfer" => PaymentMethod.BankTransfer,
        "online" => PaymentMethod.Online,
        _ => PaymentMethod.Cash
    };
}
