using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class MembershipPlanService(
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IMembershipRepositoryPort membershipRepository,
    IUnitOfWork unitOfWork) : IMembershipPlanService
{
    public async Task CreatePlanAsync(CreateMembershipPlanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Plan name cannot be empty.");

        if (dto.DurationMonth <= 0)
            throw new ArgumentException("Duration must be greater than 0.");

        if (dto.Price <= 0)
            throw new ArgumentException("Price must be greater than 0.");

        var plan = new MembershipPlanSnapshot(PlanId: 0, dto.Name, dto.DurationMonth, dto.Price);

        await membershipPlanRepository.AddAsync(plan);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteUnusedPlanAsync(int planId)
    {
        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(planId);
        if (plan is null)
            throw new KeyNotFoundException($"Membership plan with ID {planId} not found.");

        var activeMemberships = await membershipRepository.GetAllActiveMembershipReferencedOnMembershipPlan(planId);

        if (activeMemberships.Count > 0)
            throw new InvalidOperationException("Cannot delete plan. There are active memberships associated with it.");

        await membershipPlanRepository.DeleteMembershipPlanAsync(planId);
        await unitOfWork.SaveChangesAsync();
    }

    public Task<List<MembershipPlanSnapshot>> GetPlansAsync(decimal? minPrice, decimal? maxPrice)
        => membershipPlanRepository.GetPlansAsync(minPrice, maxPrice);

    public Task<MembershipPlanSnapshot?> GetPlanByIdAsync(int id)
        => membershipPlanRepository.GetMembershipPlanByIdAsync(id);
}
