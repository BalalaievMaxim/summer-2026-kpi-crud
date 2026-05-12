using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class MembershipRepository(GymManagementContext context) : IMembershipRepositoryPort
{
    public async Task AddAsync(Membership membership, CancellationToken cancellationToken = default)
    {
        var entity = new E.Membership
        {
            ClientId = membership.ClientId,
            PlanId = membership.PlanId,
            StartDate = membership.Period.Start,
            EndDate = membership.Period.End,
            IsActive = membership.IsActive
        };

        await context.Memberships.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Membership membership, CancellationToken cancellationToken = default)
    {
        var entity = await context.Memberships
            .FirstOrDefaultAsync(m => m.MembershipId == membership.Id, cancellationToken);

        if (entity is null)
            return;

        entity.ClientId = membership.ClientId;
        entity.PlanId = membership.PlanId;
        entity.StartDate = membership.Period.Start;
        entity.EndDate = membership.Period.End;
        entity.IsActive = membership.IsActive;
    }

    public async Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var list = await context.Memberships
            .AsNoTracking()
            .Where(m => m.ClientId == clientId && m.IsActive && m.EndDate >= today)
            .ToListAsync(cancellationToken);

        return list.Select(ToAggregate).ToList();
    }

    public Task<bool> HasActiveMembershipForPlanAsync(
        int clientId,
        int planId,
        DateOnly today,
        CancellationToken cancellationToken = default)
        => context.Memberships.AnyAsync(
            m => m.ClientId == clientId &&
                 m.PlanId == planId &&
                 m.IsActive &&
                 m.StartDate <= today &&
                 m.EndDate >= today,
            cancellationToken);

    public Task<bool> HasActiveMembershipsForPlanAsync(
        int planId,
        DateOnly today,
        CancellationToken cancellationToken = default)
        => context.Memberships.AnyAsync(
            m => m.PlanId == planId &&
                 m.IsActive &&
                 m.StartDate <= today &&
                 m.EndDate >= today,
            cancellationToken);

    public async Task<Membership?> GetPendingMembershipByClientAsync(int clientId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.Memberships
            .AsNoTracking()
            .Where(m => m.ClientId == clientId && m.IsActive == false)
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : ToAggregate(entity);
    }

    private static Membership ToAggregate(E.Membership m) =>
        Membership.Reconstitute(m.MembershipId, m.ClientId, m.PlanId, m.StartDate, m.EndDate, m.IsActive);
}
