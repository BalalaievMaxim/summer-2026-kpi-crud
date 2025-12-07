using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class MembershipPlanRepository(GymManagementContext context) : IMembershipPlanRepository
{
    public async Task AddAsync(Membershipplan membershipPlan)
    {
        await context.Membershipplans.AddAsync(membershipPlan);
    }
    
    public async Task<Membershipplan?> GetMembershipPlanByIdAsync(int planId)
    {
        return await context.Membershipplans
            .Where(m => m.PlanId == planId)
            .FirstAsync();
    }
}