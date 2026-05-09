namespace GymManagement.Domain.Memberships;

public interface IMembershipRepository
{
    Task<Membership?> GetByIdAsync(Guid id);
    Task<List<Membership>> GetActiveByClientAsync(Guid clientId);
    Task<bool> HasActiveMembershipAsync(Guid clientId);
    Task AddAsync(Membership membership);
    Task UpdateAsync(Membership membership);
}

public interface IMembershipPlanRepository
{
    Task<MembershipPlan?> GetByIdAsync(Guid id);
    Task<List<MembershipPlan>> GetAllAsync();
    Task AddAsync(MembershipPlan plan);
    Task DeleteAsync(Guid id);
}
