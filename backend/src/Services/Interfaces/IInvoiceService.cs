using GymManagement.DTOs;
using GymManagement.Models;
using GymManagement.Models.Enums;

namespace GymManagement.Repositories.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes);
    Task UpdatePaidInvoiceAsync(int invoiceId);
    Task<List<Invoice>> GetAllPendingInvoicesAsync(int clientId);
    Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueAnalyticsAsync();
}