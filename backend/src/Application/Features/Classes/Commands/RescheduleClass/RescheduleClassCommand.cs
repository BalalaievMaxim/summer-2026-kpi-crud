using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Classes.Commands.RescheduleClass;

public sealed record RescheduleClassCommand(
    int ClassId,
    DateTime NewStartTime,
    DateTime NewEndTime) : ICommand;
