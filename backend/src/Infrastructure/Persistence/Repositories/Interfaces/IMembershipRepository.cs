using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IMembershipRepository
{
    Task AddAsync(Membership membership);
    Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId);
    Task MarkAsActiveMembershipAsync(int membershipId);
    Task<List<Membership>> GetAllActiveMembershipReferencedOnMembershipPlan(int planId);
    Task<Membership?> GetPendingMembershipByClientAsync(int clientId);
}