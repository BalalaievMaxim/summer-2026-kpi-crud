using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Coaches.Commands.UpdateCoachSpecialization;

public sealed class UpdateCoachSpecializationCommandHandler(
    ICoachRepository coachRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCoachSpecializationCommand>
{
    public async Task Handle(UpdateCoachSpecializationCommand command, CancellationToken cancellationToken = default)
    {
        var coach = await coachRepository.GetByIdAsync(command.CoachId, cancellationToken)
            ?? throw new CoachNotFoundError(command.CoachId);

        coach.UpdateSpecialization(command.Specialization);

        await coachRepository.UpdateAsync(coach, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
