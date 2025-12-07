using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class InvoiceRepository (GymManagementContext context) : IInvoiceRepository
{
    
    public async Task MarkAsPaidAsync(int invoiceId)
    {
        await context.Invoices
            .Where(i => i.InvoiceId == invoiceId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.Status, nameof(PaymentStatus.Paid).ToLower()));
    }

    public async Task AddAsync(Invoice invoice)
    {
        await context.Invoices.AddAsync(invoice);
    }
}