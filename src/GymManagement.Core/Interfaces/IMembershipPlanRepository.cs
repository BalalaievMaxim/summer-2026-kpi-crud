using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipPlanRepository
{
    public Task AddAsync (MembershipPlan membershipPlan);
    
    public Task<MembershipPlan?> GetMembershipPlanByIdAsync(int planId);
    
    public Task DeleteMembershipPlanAsync(int planId);

    public Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max);
}