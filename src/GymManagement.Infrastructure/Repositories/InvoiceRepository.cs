using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using GymManagement.Infrastructure.DTOs;
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

    public async Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueByPlanAsync()
    {
        var sql = @"
        SELECT
            TO_CHAR(i.date, 'YYYY-MM') AS RevenueMonth,
            mp.name AS PlanName,
            SUM(i.amount) AS TotalRevenue
        FROM Invoice i
        JOIN Membership m ON i.client_id = m.client_id
            AND i.date BETWEEN m.start_date AND m.end_date
        JOIN MembershipPlan mp ON m.plan_id = mp.plan_id
        WHERE i.status = 'paid'
        GROUP BY TO_CHAR(i.date, 'YYYY-MM'), mp.name
        ORDER BY RevenueMonth DESC, TotalRevenue DESC";
        
        return await context.Database
            .SqlQuery<TotalMembershipRevenueDto>(System.Runtime.CompilerServices.FormattableStringFactory.Create(sql))
            .ToListAsync();
    }
}