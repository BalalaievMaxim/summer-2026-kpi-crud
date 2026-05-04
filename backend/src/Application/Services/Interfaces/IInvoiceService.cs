using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Entities.Enums;

namespace GymManagement.Application.Services.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes);
    Task UpdatePaidInvoiceAsync(int invoiceId);
    Task<List<Invoice>> GetAllPendingInvoicesAsync(int clientId);
    Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueAnalyticsAsync();
}