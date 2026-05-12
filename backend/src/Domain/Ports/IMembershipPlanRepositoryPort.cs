using GymManagement.Domain.Memberships;

namespace GymManagement.Domain.Ports;

public interface IMembershipPlanRepositoryPort
{
    Task AddAsync(MembershipPlan plan, CancellationToken cancellationToken = default);
    Task<MembershipPlan?> GetByIdAsync(int planId, CancellationToken cancellationToken = default);
    Task DeleteMembershipPlanAsync(int planId, CancellationToken cancellationToken = default);
    Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max, CancellationToken cancellationToken = default);
}
