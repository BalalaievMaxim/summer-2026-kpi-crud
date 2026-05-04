using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipPlanService
{
    Task CreatePlanAsync(CreateMembershipPlanDto dto);
    Task DeleteUnusedPlanAsync(int planId);
    Task<List<MembershipPlan>> GetPlansAsync(decimal? minPrice, decimal? maxPrice);
    Task<MembershipPlan?> GetPlanByIdAsync(int id);
}