using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Coaches.Commands.DeleteCoach;

public sealed class DeleteCoachCommandHandler(
    ICoachRepository coachRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCoachCommand>
{
    public async Task Handle(DeleteCoachCommand command, CancellationToken cancellationToken = default)
    {
        if (!await coachRepository.ExistsAsync(command.CoachId, cancellationToken))
            throw new CoachNotFoundError(command.CoachId);

        if (await coachRepository.HasUpcomingClassesWithEnrollmentsAsync(command.CoachId, cancellationToken))
            throw new CoachHasFutureClassesError(command.CoachId);

        await coachRepository.DeleteUpcomingClassesByCoachAsync(command.CoachId, cancellationToken);
        await coachRepository.DeleteAsync(command.CoachId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
