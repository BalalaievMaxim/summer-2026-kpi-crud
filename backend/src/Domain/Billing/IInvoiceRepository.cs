namespace GymManagement.Domain.Billing;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<List<Invoice>> GetByClientAsync(Guid clientId);
    Task AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
}
