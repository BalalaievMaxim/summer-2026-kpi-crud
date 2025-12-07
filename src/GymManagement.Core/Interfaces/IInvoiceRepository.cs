using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IInvoiceRepository
{
    public Task MarkAsPaidAsync(int invoiceId);
    
    public Task AddAsync(Invoice invoice);
}