using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Auth.Queries.LoginClient;

public sealed record LoginClientQuery(string Email, string Password) : IQuery<AuthResultDto>;
