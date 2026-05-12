using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipPlanReadRepository
{
    Task<MembershipPlanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<MembershipPlanDto>> GetPlansAsync(decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default);
}