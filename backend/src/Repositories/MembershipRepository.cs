using GymManagement.Models;
using GymManagement.Repositories.Interfaces;
using GymManagement.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Repositories;

public class MembershipRepository(GymManagementContext context) : IMembershipRepository
{
    public async Task AddAsync(Membership membership)
    {
        await context.Memberships.AddAsync(membership);
    }
    
    public async Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == true && m.EndDate > today)
            .ToListAsync();
    }

    public async Task MarkAsActiveMembershipAsync(int membershipId)
    {
        await context.Memberships
            .Where(m => m.MembershipId == membershipId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsActive, true));
    }

    public async Task<List<Membership>> GetAllActiveMembershipReferencedOnMembershipPlan(int planId)
    {
        return await context.Memberships
            .Where(m => m.PlanId == planId && m.IsActive == true)
            .ToListAsync();
    }
    
    public async Task<Membership?> GetPendingMembershipByClientAsync(int clientId)
    {
        return await context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == false)
            .OrderByDescending(m => m.StartDate) 
            .FirstOrDefaultAsync();
    }
}