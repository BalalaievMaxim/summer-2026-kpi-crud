using GymManagement.DTOs;
using GymManagement.Models;

namespace GymManagement.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllClientInvoicesAsync(int clientId);
    Task<List<Invoice>> GetPendingInvoicesAsync(int clientId); 
    Task<Invoice?> GetInvoiceAsync(int invoiceId);
    Task AddAsync(Invoice invoice);
    Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueByPlanAsync();
}