using GymManagement.Domain.Billing;
using GymManagement.Domain.Queries;

namespace GymManagement.Application.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceRecord> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes);
    Task UpdatePaidInvoiceAsync(int invoiceId);
    Task<List<InvoiceRecord>> GetAllPendingInvoicesAsync(int clientId);
    Task<List<TotalMembershipRevenueRow>> GetMonthlyRevenueAnalyticsAsync();
}
