using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;

public sealed class DeleteMembershipPlanCommandHandler(
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IMembershipRepositoryPort membershipRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMembershipPlanCommand>
{
    public async Task Handle(DeleteMembershipPlanCommand command, CancellationToken cancellationToken = default)
    {
        var plan = await membershipPlanRepository.GetByIdAsync(command.PlanId, cancellationToken);
        if (plan is null)
            throw new MembershipPlanNotFoundError(command.PlanId);

        var hasActiveMemberships = await membershipRepository.HasActiveMembershipsForPlanAsync(
            command.PlanId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            cancellationToken);

        if (hasActiveMemberships)
            throw new MembershipPlanInUseError(command.PlanId);

        await membershipPlanRepository.DeleteMembershipPlanAsync(command.PlanId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
