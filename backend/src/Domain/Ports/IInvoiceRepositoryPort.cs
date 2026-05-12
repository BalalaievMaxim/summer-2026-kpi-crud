using GymManagement.Domain.Billing;

namespace GymManagement.Domain.Ports;

public interface IInvoiceRepositoryPort
{
    Task<List<InvoiceRecord>> GetAllClientInvoicesAsync(int clientId, CancellationToken cancellationToken = default);
    Task<List<InvoiceRecord>> GetPendingInvoicesAsync(int clientId, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByIdAsync(int invoiceId, CancellationToken cancellationToken = default);

    Task<int> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
