using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public sealed class CreateMembershipPlanCommandHandler(
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateMembershipPlanCommand>
{
    public async Task Handle(CreateMembershipPlanCommand command, CancellationToken cancellationToken = default)
    {
        var plan = MembershipPlan.Create(
            command.Name,
            command.DurationMonths,
            command.Price);

        await membershipPlanRepository.AddAsync(plan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
