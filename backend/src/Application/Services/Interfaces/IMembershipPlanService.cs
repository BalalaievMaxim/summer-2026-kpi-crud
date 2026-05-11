using GymManagement.Application.DTOs;
using GymManagement.Domain.Memberships;

namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipPlanService
{
    Task CreatePlanAsync(CreateMembershipPlanDto dto);
    Task DeleteUnusedPlanAsync(int planId);
    Task<List<MembershipPlanSnapshot>> GetPlansAsync(decimal? minPrice, decimal? maxPrice);
    Task<MembershipPlanSnapshot?> GetPlanByIdAsync(int id);
}
