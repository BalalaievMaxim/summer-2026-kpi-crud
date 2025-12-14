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