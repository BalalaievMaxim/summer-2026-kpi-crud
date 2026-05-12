using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Classes.Commands.CreateClass;

public sealed record CreateClassCommand(
    int ClassTypeId,
    int CoachId,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity) : ICommand<int>;
