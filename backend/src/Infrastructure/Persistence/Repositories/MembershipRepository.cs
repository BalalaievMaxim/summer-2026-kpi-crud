using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class MembershipRepository(GymManagementContext context) : IMembershipRepositoryPort
{
    public async Task AddAsync(MembershipRecord membership, CancellationToken cancellationToken = default)
    {
        var entity = new E.Membership
        {
            ClientId = membership.ClientId,
            PlanId = membership.PlanId,
            StartDate = membership.StartDate,
            EndDate = membership.EndDate,
            IsActive = membership.IsActive
        };

        await context.Memberships.AddAsync(entity, cancellationToken);
    }

    public async Task<List<MembershipRecord>> GetActiveMembershipsByClientAsync(int clientId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var list = await context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive && m.EndDate > today)
            .ToListAsync(cancellationToken);

        return list.Select(ToRecord).ToList();
    }

    public async Task MarkAsActiveMembershipAsync(int membershipId, CancellationToken cancellationToken = default)
    {
        await context.Memberships
            .Where(m => m.MembershipId == membershipId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsActive, true), cancellationToken);
    }

    public async Task<List<MembershipRecord>> GetAllActiveMembershipReferencedOnMembershipPlan(int planId,
        CancellationToken cancellationToken = default)
    {
        var list = await context.Memberships
            .Where(m => m.PlanId == planId && m.IsActive)
            .ToListAsync(cancellationToken);

        return list.Select(ToRecord).ToList();
    }

    public async Task<MembershipRecord?> GetPendingMembershipByClientAsync(int clientId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == false)
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : ToRecord(entity);
    }

    private static MembershipRecord ToRecord(E.Membership m) =>
        new(m.MembershipId, m.ClientId, m.PlanId, m.StartDate, m.EndDate, m.IsActive);
}
