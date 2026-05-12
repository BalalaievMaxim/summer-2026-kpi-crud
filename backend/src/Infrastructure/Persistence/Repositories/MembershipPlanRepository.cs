using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class MembershipPlanRepository(GymManagementContext context) : IMembershipPlanRepositoryPort
{
    public async Task AddAsync(MembershipPlan plan, CancellationToken cancellationToken = default)
    {
        var entity = new E.MembershipPlan
        {
            Name = plan.Name,
            DurationMonths = plan.DurationMonths,
            Price = plan.Price.Amount
        };

        await context.Membershipplans.AddAsync(entity, cancellationToken);
    }

    public async Task<MembershipPlan?> GetByIdAsync(int planId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.Membershipplans
            .AsNoTracking()
            .Include(p => p.PlanAccesses)
            .FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);

        return entity is null ? null : ToAggregate(entity);
    }

    public async Task DeleteMembershipPlanAsync(int planId, CancellationToken cancellationToken = default)
    {
        var entity = await context.Membershipplans.FindAsync([planId], cancellationToken);
        if (entity is not null)
            context.Membershipplans.Remove(entity);
    }

    public async Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max,
        CancellationToken cancellationToken = default)
    {
        var query = context.Membershipplans.AsNoTracking().AsQueryable();
        if (min.HasValue) query = query.Where(p => p.Price >= min.Value);
        if (max.HasValue) query = query.Where(p => p.Price <= max.Value);

        var list = await query.ToListAsync(cancellationToken);
        return list.Select(ToAggregate).ToList();
    }

    private static MembershipPlan ToAggregate(E.MembershipPlan p) =>
        MembershipPlan.Reconstitute(
            p.PlanId,
            p.Name,
            p.DurationMonths,
            p.Price,
            p.PlanAccesses.Select(access => access.ZoneId));
}
