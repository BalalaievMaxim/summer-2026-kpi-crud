using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipPlanRepository
{
    public Task AddAsync (Membershipplan membershipPlan);
}