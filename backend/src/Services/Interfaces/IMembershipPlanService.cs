using GymManagement.DTOs;
using GymManagement.Models;

namespace GymManagement.Repositories.Interfaces;

public interface IMembershipPlanService
{
    Task CreatePlanAsync(CreateMembershipPlanDto dto);
    Task DeleteUnusedPlanAsync(int planId);
    Task<List<MembershipPlan>> GetPlansAsync(decimal? minPrice, decimal? maxPrice);
    Task<MembershipPlan?> GetPlanByIdAsync(int id);
}