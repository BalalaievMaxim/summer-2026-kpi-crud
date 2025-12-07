using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipRepository
{
    public Task AddPlanAsync(Membershipplan plan);
    
    public Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId);
}