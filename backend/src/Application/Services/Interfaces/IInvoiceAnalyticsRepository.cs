using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IInvoiceAnalyticsRepository
{
    Task<List<TotalMembershipRevenueRow>> GetMonthlyRevenueByPlanAsync(CancellationToken cancellationToken = default);
}
