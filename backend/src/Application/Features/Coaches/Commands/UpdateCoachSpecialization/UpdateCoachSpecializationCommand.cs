using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Coaches.Commands.UpdateCoachSpecialization;

public sealed record UpdateCoachSpecializationCommand(int CoachId, string Specialization) : ICommand;
