using GymManagement.Models;

namespace GymManagement.Repositories.Interfaces;

public interface IMembershipPlanRepository
{
    Task AddAsync(MembershipPlan membershipPlan);
    Task<MembershipPlan?> GetMembershipPlanByIdAsync(int planId);
    Task DeleteMembershipPlanAsync(int planId);
    Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max);
}