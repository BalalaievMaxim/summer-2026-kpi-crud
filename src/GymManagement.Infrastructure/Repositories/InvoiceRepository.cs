using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class InvoiceRepository (GymManagementContext context) : IInvoiceRepository
{
    public async Task<List<Invoice>> GetAllClientInvoicesAsync(int clientId)
    {
        return await context.Invoices.Where(i => i.ClientId == clientId).ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceAsync(int clientId, int invoiceId)
    {
        return await context.Invoices.Where(i => i.ClientId == clientId && i.InvoiceId == invoiceId).FirstOrDefaultAsync();
    }
    public async Task AddAsync(Invoice invoice)
    {
        await context.Invoices.AddAsync(invoice);
    }

    public async Task MarkAsPayedAsync(int invoiceId)
    {
        await context.Invoices
            .Where(invoice => invoice.InvoiceId == invoiceId)
            .ExecuteUpdateAsync(i => i
                .SetProperty(e => e.Status, nameof(PaymentStatus.Paid).ToLower()));
    }
}