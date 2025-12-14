using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class MembershipPlanRepository(GymManagementContext context) : IMembershipPlanRepository
{
    public async Task AddAsync(MembershipPlan membershipPlan)
    {
        await context.Membershipplans.AddAsync(membershipPlan);
    }
    
    public async Task<MembershipPlan?> GetMembershipPlanByIdAsync(int planId)
    {
        return await context.Membershipplans
            .Where(m => m.PlanId == planId)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteMembershipPlanAsync(int planId)
    {
        await context.Membershipplans
            .Where(m => m.PlanId == planId)
            .ExecuteDeleteAsync();
        await AddAsync(await context.Membershipplans.Where(m => m.PlanId == planId).FirstAsync());
    }
    
    public async Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max)
    {
        var query = await context.Membershipplans.AsNoTracking().ToListAsync();

        if (min.HasValue) query = query.Where(p => p.Price >= min.Value).ToList();;
        if (max.HasValue) query = query.Where(p => p.Price <= max.Value).ToList();

        query = query.OrderBy(p => p.Price).ToList();

        return query.ToList();
    }
}