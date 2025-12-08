using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipPlanRepository
{
    public Task AddAsync (Membershipplan membershipPlan);
    
    public Task<Membershipplan?> GetMembershipPlanByIdAsync(int planId);
    
    public Task DeleteMembershipPlanAsync(int planId);

    public Task<List<Membershipplan>> GetPlansAsync(decimal? min, decimal? max);
}