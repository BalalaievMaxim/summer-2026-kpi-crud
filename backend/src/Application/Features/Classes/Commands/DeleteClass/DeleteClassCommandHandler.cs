using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Classes.Commands.DeleteClass;

public sealed class DeleteClassCommandHandler(
    IClassRepositoryPort classRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteClassCommand>
{
    public async Task Handle(DeleteClassCommand command, CancellationToken cancellationToken = default)
    {
        var classEntity = await classRepository.GetByIdAsync(command.ClassId, cancellationToken)
            ?? throw new ClassNotFoundError(command.ClassId);

        classEntity.EnsureCanBeDeleted();

        await classRepository.DeleteAsync(command.ClassId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
