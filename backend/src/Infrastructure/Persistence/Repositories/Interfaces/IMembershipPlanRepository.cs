using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IMembershipPlanRepository
{
    Task AddAsync(MembershipPlan membershipPlan);
    Task<MembershipPlan?> GetMembershipPlanByIdAsync(int planId);
    Task DeleteMembershipPlanAsync(int planId);
    Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max);
}