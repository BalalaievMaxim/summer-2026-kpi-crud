using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;

namespace GymManagement.Application.Features.Auth.Commands.LoginClient;

public sealed record LoginClientCommand(string Email, string Password) : ICommand<ClientDto>;
