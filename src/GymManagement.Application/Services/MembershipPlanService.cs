using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;

namespace GymManagement.Application.Services;

public class MembershipPlanService(
    IMembershipPlanRepository membershipPlanRepository, 
    IMembershipRepository membershipRepository, 
    IUnitOfWork unitOfWork)
{
    public async Task CreatePlanAsync(CreateMembershipPlanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Plan name cannot be empty.");
        
        if (dto.DurationMonth <= 0)
            throw new ArgumentException("Duration must be greater than 0.");
            
        if (dto.Price <= 0)
            throw new ArgumentException("Price must be greater than 0.");

        var plan = new MembershipPlan
        {
            Name = dto.Name,
            DurationMonths = dto.DurationMonth,
            Price = dto.Price
        };

        await membershipPlanRepository.AddAsync(plan);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteUnusedPlanAsync(int planId)
    {
        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(planId);
        if (plan == null)
        {
            throw new KeyNotFoundException($"Membership plan with ID {planId} not found.");
        }

        var activeMemberships = await membershipRepository.GetAllActiveMembershipReferencedOnMembershipPlan(planId);
        
        if (activeMemberships.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete plan. There are active memberships associated with it.");
        }

        await membershipPlanRepository.DeleteMembershipPlanAsync(planId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<List<MembershipPlan>> GetPlansAsync(decimal? minPrice, decimal? maxPrice)
    {
        return await membershipPlanRepository.GetPlansAsync(minPrice, maxPrice);
    }
}