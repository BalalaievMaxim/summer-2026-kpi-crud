using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;

namespace GymManagement.Infrastructure.Repositories;

public class MembershipPlanRepository(GymManagementContext context) : IMembershipPlanRepository
{
    public async Task AddAsync(Membershipplan membershipPlan)
    {
        await context.Membershipplans.AddAsync(membershipPlan);
    }
}