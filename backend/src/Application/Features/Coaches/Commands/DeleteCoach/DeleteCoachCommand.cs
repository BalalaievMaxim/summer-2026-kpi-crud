using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Coaches.Commands.DeleteCoach;

public sealed record DeleteCoachCommand(int CoachId) : ICommand;
