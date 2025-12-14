using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipRepository
{
    Task AddAsync(Membership membership);
    Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId);
    Task MarkAsActiveMembershipAsync(int membershipId);
    Task<List<Membership>> GetAllActiveMembershipReferencedOnMembershipPlan(int planId);
}