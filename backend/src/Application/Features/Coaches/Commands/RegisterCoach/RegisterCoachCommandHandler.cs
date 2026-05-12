using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Coaches.Commands.RegisterCoach;

public sealed class RegisterCoachCommandHandler(
    ICoachRepository coachRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<RegisterCoachCommand, int>
{
    public async Task<int> Handle(RegisterCoachCommand command, CancellationToken cancellationToken = default)
    {
        if (await coachRepository.ExistsByEmailAsync(command.Email, cancellationToken))
            throw new CoachEmailAlreadyExistsError(command.Email);

        var coach = Coach.Create(command.Name, command.Email, command.Specialization, command.Password, passwordHasher);
        return await coachRepository.AddAsync(coach, cancellationToken);
    }
}
