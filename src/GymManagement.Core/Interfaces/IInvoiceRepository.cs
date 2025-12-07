using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IInvoiceRepository
{
    public Task<List<Invoice>> GetAllClientInvoicesAsync(int clientId);
    public Task<Invoice?> GetInvoiceAsync(int clientId, int invoiceId);
    public Task MarkAsPayedAsync(int invoiceId);
    public Task AddAsync(Invoice invoice);
}