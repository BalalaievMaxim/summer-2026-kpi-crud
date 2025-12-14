using GymManagement.Core.Entities;
using GymManagement.Core.Enums;

namespace GymManagement.Core.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes);
    Task UpdatePaidInvoiceAsync(int invoiceId);
    Task<List<Invoice>> GetAllPendingInvoicesAsync(int clientId);
}