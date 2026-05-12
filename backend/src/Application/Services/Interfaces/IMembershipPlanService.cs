using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipPlanService
{
    Task CreatePlanAsync(CreateMembershipPlanDto dto);
    Task DeleteUnusedPlanAsync(int planId);
    Task<List<MembershipPlanDto>> GetPlansAsync(decimal? minPrice, decimal? maxPrice);
    Task<MembershipPlanDto?> GetPlanByIdAsync(int id);
}
