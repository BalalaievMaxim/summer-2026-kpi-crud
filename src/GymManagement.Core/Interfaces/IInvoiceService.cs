using GymManagement.Core.Entities;
using GymManagement.Core.Enums;

namespace GymManagement.Core.Interfaces;

public interface IInvoiceService
{
    public Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId,
        string? notes);

    public Task UpdatePaidInvoiceAsync(int clientId, int invoiceId);

    public  Task<List<Invoice>> GetAllPendingInvoicesAsync(int clientId);
}