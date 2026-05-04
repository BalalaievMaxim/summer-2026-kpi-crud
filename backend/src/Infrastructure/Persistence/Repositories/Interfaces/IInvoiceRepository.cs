using GymManagement.Infrastructure.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllClientInvoicesAsync(int clientId);
    Task<List<Invoice>> GetPendingInvoicesAsync(int clientId); 
    Task<Invoice?> GetInvoiceAsync(int invoiceId);
    Task AddAsync(Invoice invoice);
    Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueByPlanAsync();
}