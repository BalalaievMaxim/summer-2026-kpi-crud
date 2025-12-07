using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class MembershipRepository(GymManagementContext context) : IMembershipRepository
{
    public async Task AddPlanAsync(Membershipplan plan)
    {
        await context.Membershipplans.AddAsync(plan);
        await context.SaveChangesAsync();
    }

    public async Task<List<Membership>> GetActiveMembershipsByClientAsync(int clientId)
    {
        return await context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == true && m.EndDate > DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync();
    }
}