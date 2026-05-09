using DomainInvoice = GymManagement.Domain.Billing.Invoice;
using DomainIInvoiceRepository = GymManagement.Domain.Billing.IInvoiceRepository;
using EfInvoice = GymManagement.Infrastructure.Persistence.Entities.Invoice;
using OldIInvoiceRepository = GymManagement.Infrastructure.Persistence.Repositories.Interfaces.IInvoiceRepository;
using GymManagement.Domain.Billing;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : OldIInvoiceRepository, DomainIInvoiceRepository
{
    private readonly GymManagementContext _context;

    public InvoiceRepository(GymManagementContext context)
    {
        _context = context;
    }

    // ── старий інтерфейс ──
    public async Task<List<EfInvoice>> GetAllClientInvoicesAsync(int clientId)
    {
        return await _context.Invoices
            .Where(i => i.ClientId == clientId)
            .ToListAsync();
    }

    public async Task<List<EfInvoice>> GetPendingInvoicesAsync(int clientId)
    {
        return await _context.Invoices
            .Where(i => i.ClientId == clientId && i.Status == "pending")
            .ToListAsync();
    }

    public async Task<EfInvoice?> GetInvoiceAsync(int invoiceId)
    {
        return await _context.Invoices.FindAsync(invoiceId);
    }

    public async Task AddAsync(EfInvoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
    }

    public async Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueByPlanAsync()
    {
        return await _context.Invoices
            .Where(i => i.Status == "paid")
            .Join(_context.Memberships,
                i => i.ClientId,
                m => m.ClientId,
                (i, m) => new { Invoice = i, Membership = m })
            .Join(_context.Membershipplans,
                im => im.Membership.PlanId,
                p => p.PlanId,
                (im, p) => new { im.Invoice, Plan = p })
            .GroupBy(x => new { x.Invoice.Date.Month, x.Plan.Name })
            .Select(g => new TotalMembershipRevenueDto
            {
                RevenueMonth = g.Key.Month.ToString(),
                PlanName = g.Key.Name,
                TotalRevenue = g.Sum(x => x.Invoice.Amount)
            })
            .ToListAsync();
    }

    // ── новий доменний інтерфейс ──
    public async Task<DomainInvoice?> GetByIdAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Invoices.FindAsync(intId);
        return entity is null ? null : InvoiceMapper.ToDomain(entity);
    }

    public async Task<List<DomainInvoice>> GetByClientAsync(Guid clientId)
    {
        var intClientId = GuidToInt(clientId);
        var entities = await _context.Invoices
            .Where(i => i.ClientId == intClientId)
            .ToListAsync();
        return entities.Select(InvoiceMapper.ToDomain).ToList();
    }

    public async Task AddAsync(DomainInvoice invoice)
    {
        var entity = InvoiceMapper.ToEntity(invoice);
        await _context.Invoices.AddAsync(entity);
    }

    public async Task UpdateAsync(DomainInvoice invoice)
    {
        var entity = InvoiceMapper.ToEntity(invoice);
        _context.Invoices.Update(entity);
        await Task.CompletedTask;
    }

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
