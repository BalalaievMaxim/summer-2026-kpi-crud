using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Exceptions;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Classes.Commands.CreateClass;

public sealed class CreateClassCommandHandler(
    IClassRepositoryPort classRepository,
    IClassTypeRepositoryPort classTypeRepository,
    ClassFactory classFactory) : ICommandHandler<CreateClassCommand, int>
{
    public async Task<int> Handle(CreateClassCommand command, CancellationToken cancellationToken = default)
    {
        if (!await classTypeRepository.ExistsAsync(command.ClassTypeId, cancellationToken))
            throw new NotFoundException($"ClassType with ID {command.ClassTypeId} not found.");

        var classEntity = await classFactory.CreateAsync(
            command.ClassTypeId,
            command.CoachId,
            new DateTimeOffset(DateTime.SpecifyKind(command.StartTime, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(command.EndTime, DateTimeKind.Utc)),
            command.Capacity,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return await classRepository.AddAsync(classEntity, cancellationToken);
    }
}
