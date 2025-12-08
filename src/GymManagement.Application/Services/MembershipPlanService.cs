using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Repositories;

namespace GymManagement.Application.Services;

public class MembershipPlanService(IMembershipPlanRepository membershipPlanRepository, IMembershipRepository membershipRepository, IUnitOfWork unitOfWork)
{
    public async Task CreatePlanAsync(CreateMembershipPlanDto dto)
    {
        
        var plan = new Membershipplan()
        {
            Name = dto.Name,
            DurationMonths = dto.DurationMonth,
            Price = dto.Price
        };

        if (plan.DurationMonths > 0 && plan.Name != String.Empty && plan.Price > 0)
        {
            await membershipPlanRepository.AddAsync(plan);
        }
    }

    public async Task DeleteUnusedPlanAsync(int planId)
    {
        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(planId);

        List <Membership> activeMemberships = await membershipRepository.GetAllActiveMembershipReferencedOnMembershipPlan(planId);
        if (activeMemberships.Count == 0)
        {
            await membershipPlanRepository.DeleteMembershipPlanAsync(planId);
            await unitOfWork.SaveChangesAsync();
        }
        else
        {
            throw new Exception("The plan has active memberships");
        }
        
    }
}