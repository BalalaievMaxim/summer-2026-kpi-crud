using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class MembershipRepository(GymManagementContext context) : IMembershipRepository
{
    public async Task AddAsync(Membership membership)
    {
        await context.Memberships.AddAsync(membership);
    }
    
    public async Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId)
    {
        return await context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == true && m.EndDate > DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync();
    }

    public async Task<Membershipplan> GetMembershipPlanByIdAsync(int planId)
    {
        return await context.Membershipplans
            .Where(m => m.PlanId == planId)
            .FirstAsync();
    }
}