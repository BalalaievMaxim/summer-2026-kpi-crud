using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipPlanRepository(GymManagementContext context) : IMembershipPlanRepository
{
    public async Task AddAsync(MembershipPlan membershipPlan)
    {
        await context.Membershipplans.AddAsync(membershipPlan);
    }
    
    public async Task<MembershipPlan?> GetMembershipPlanByIdAsync(int planId)
    {
        return await context.Membershipplans
            .FirstOrDefaultAsync(m => m.PlanId == planId);
    }

    public async Task DeleteMembershipPlanAsync(int planId)
    {
        await context.Membershipplans
            .Where(m => m.PlanId == planId)
            .ExecuteDeleteAsync();
    }
    
    public async Task<List<MembershipPlan>> GetPlansAsync(decimal? min, decimal? max)
    {
        var query = context.Membershipplans.AsNoTracking().AsQueryable();

        if (min.HasValue) 
            query = query.Where(p => p.Price >= min.Value);
        
        if (max.HasValue) 
            query = query.Where(p => p.Price <= max.Value);

        return await query.OrderBy(p => p.Price).ToListAsync();
    }
}