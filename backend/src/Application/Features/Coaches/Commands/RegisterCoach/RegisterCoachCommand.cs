using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Coaches.Commands.RegisterCoach;

public sealed record RegisterCoachCommand(
    string Name,
    string Email,
    string Specialization,
    string Password) : ICommand<int>;
