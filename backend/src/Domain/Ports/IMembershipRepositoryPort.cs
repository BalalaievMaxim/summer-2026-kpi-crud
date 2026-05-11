using GymManagement.Domain.Memberships;

namespace GymManagement.Domain.Ports;

public interface IMembershipRepositoryPort
{
    Task AddAsync(MembershipRecord membership, CancellationToken cancellationToken = default);
    Task<List<MembershipRecord>> GetActiveMembershipsByClientAsync(int clientId, CancellationToken cancellationToken = default);
    Task MarkAsActiveMembershipAsync(int membershipId, CancellationToken cancellationToken = default);
    Task<List<MembershipRecord>> GetAllActiveMembershipReferencedOnMembershipPlan(int planId, CancellationToken cancellationToken = default);
    Task<MembershipRecord?> GetPendingMembershipByClientAsync(int clientId, CancellationToken cancellationToken = default);
}
