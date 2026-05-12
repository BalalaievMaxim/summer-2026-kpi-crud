using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class MembershipPlanService(
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IMembershipRepositoryPort membershipRepository,
    IUnitOfWork unitOfWork) : IMembershipPlanService
{
    public async Task CreatePlanAsync(CreateMembershipPlanDto dto)
    {
        var plan = MembershipPlan.Create(
            dto.Name,
            dto.DurationMonth,
            dto.Price);

        await membershipPlanRepository.AddAsync(plan);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteUnusedPlanAsync(int planId)
    {
        var plan = await membershipPlanRepository.GetByIdAsync(planId);
        if (plan is null)
            throw new MembershipPlanNotFoundError(planId);

        var hasActiveMemberships = await membershipRepository.HasActiveMembershipsForPlanAsync(
            planId,
            DateOnly.FromDateTime(DateTime.UtcNow));

        if (hasActiveMemberships)
            throw new MembershipPlanInUseError(planId);

        await membershipPlanRepository.DeleteMembershipPlanAsync(planId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<List<MembershipPlanDto>> GetPlansAsync(decimal? minPrice, decimal? maxPrice)
    {
        var plans = await membershipPlanRepository.GetPlansAsync(minPrice, maxPrice);
        return plans.Select(ToDto).ToList();
    }

    public async Task<MembershipPlanDto?> GetPlanByIdAsync(int id)
    {
        var plan = await membershipPlanRepository.GetByIdAsync(id);
        return plan is null ? null : ToDto(plan);
    }

    private static MembershipPlanDto ToDto(MembershipPlan plan)
        => new(plan.Id, plan.Name, plan.DurationMonths, plan.Price.Amount);
}
