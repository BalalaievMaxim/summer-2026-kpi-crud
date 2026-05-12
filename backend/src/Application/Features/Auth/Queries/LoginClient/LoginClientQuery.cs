using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;

namespace GymManagement.Application.Features.Auth.Queries.LoginClient;

public sealed record LoginClientQuery(string Email, string Password) : IQuery<ClientDto>;
