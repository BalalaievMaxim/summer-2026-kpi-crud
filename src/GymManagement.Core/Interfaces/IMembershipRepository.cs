using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipRepository
{
    public Task AddAsync(Membership membership);
    
    public Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId);
    
    public Task<Membershipplan> GetMembershipPlanByIdAsync(int planId);
}