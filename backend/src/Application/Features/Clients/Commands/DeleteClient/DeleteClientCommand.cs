using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(int ClientId) : ICommand;
