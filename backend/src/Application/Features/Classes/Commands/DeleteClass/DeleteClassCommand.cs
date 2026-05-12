using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Classes.Commands.DeleteClass;

public sealed record DeleteClassCommand(int ClassId) : ICommand;
