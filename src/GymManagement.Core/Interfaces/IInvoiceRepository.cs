using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IInvoiceRepository
{
    public Task CreateInvoiceAsync(Invoice invoice);

    public Task MarkAsPaidAsync(int invoiceId);
}