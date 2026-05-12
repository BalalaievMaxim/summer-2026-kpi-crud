using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Application.Features.Classes.Commands.RescheduleClass;

public sealed class RescheduleClassCommandHandler(
    IClassRepositoryPort classRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RescheduleClassCommand>
{
    public async Task Handle(RescheduleClassCommand command, CancellationToken cancellationToken = default)
    {
        var classEntity = await classRepository.GetByIdAsync(command.ClassId, cancellationToken)
            ?? throw new ClassNotFoundError(command.ClassId);

        var newRange = TimeRange.Create(
            new DateTimeOffset(DateTime.SpecifyKind(command.NewStartTime, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(command.NewEndTime, DateTimeKind.Utc)));

        var hasConflict = await classRepository.HasOverlappingClassAsync(
            classEntity.CoachId,
            newRange,
            command.ClassId,
            cancellationToken);

        classEntity.Reschedule(newRange, hasConflict, DateTimeOffset.UtcNow);

        await classRepository.UpdateAsync(classEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
