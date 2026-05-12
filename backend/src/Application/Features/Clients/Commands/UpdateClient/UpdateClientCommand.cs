using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(int ClientId, string Email, string Phone) : ICommand;
