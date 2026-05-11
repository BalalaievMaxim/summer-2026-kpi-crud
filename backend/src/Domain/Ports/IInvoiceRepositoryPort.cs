using GymManagement.Domain.Billing;
using GymManagement.Domain.Queries;

namespace GymManagement.Domain.Ports;

public interface IInvoiceRepositoryPort
{
    Task<List<InvoiceRecord>> GetAllClientInvoicesAsync(int clientId, CancellationToken cancellationToken = default);
    Task<List<InvoiceRecord>> GetPendingInvoicesAsync(int clientId, CancellationToken cancellationToken = default);
    Task<InvoiceRecord?> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task AddAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int invoiceId, string status, CancellationToken cancellationToken = default);
    Task<List<TotalMembershipRevenueRow>> GetMonthlyRevenueByPlanAsync(CancellationToken cancellationToken = default);
}
