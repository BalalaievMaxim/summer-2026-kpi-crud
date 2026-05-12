using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Clients.ReadModels;

namespace GymManagement.Application.Features.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(int ClientId) : IQuery<ClientDto?>;
