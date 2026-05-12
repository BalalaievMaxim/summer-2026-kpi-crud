using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Clients.Commands.RegisterClient;

public sealed record RegisterClientCommand(
    string Name,
    string Email,
    string Phone,
    string Password) : ICommand<AuthResultDto>;