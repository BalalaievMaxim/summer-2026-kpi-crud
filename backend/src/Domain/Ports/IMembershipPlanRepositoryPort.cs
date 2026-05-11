using GymManagement.Domain.Memberships;

namespace GymManagement.Domain.Ports;

public interface IMembershipPlanRepositoryPort
{
    Task AddAsync(MembershipPlanSnapshot plan, CancellationToken cancellationToken = default);
    Task<MembershipPlanSnapshot?> GetMembershipPlanByIdAsync(int planId, CancellationToken cancellationToken = default);
    Task DeleteMembershipPlanAsync(int planId, CancellationToken cancellationToken = default);
    Task<List<MembershipPlanSnapshot>> GetPlansAsync(decimal? min, decimal? max, CancellationToken cancellationToken = default);
}
