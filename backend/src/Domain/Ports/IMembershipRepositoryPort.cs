using GymManagement.Domain.Memberships;

namespace GymManagement.Domain.Ports;

public interface IMembershipRepositoryPort
{
    Task AddAsync(Membership membership, CancellationToken cancellationToken = default);
    Task UpdateAsync(Membership membership, CancellationToken cancellationToken = default);
    Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveMembershipForPlanAsync(int clientId, int planId, DateOnly today, CancellationToken cancellationToken = default);
    Task<bool> HasActiveMembershipsForPlanAsync(int planId, DateOnly today, CancellationToken cancellationToken = default);
    Task<Membership?> GetPendingMembershipByClientAsync(int clientId, CancellationToken cancellationToken = default);
}
