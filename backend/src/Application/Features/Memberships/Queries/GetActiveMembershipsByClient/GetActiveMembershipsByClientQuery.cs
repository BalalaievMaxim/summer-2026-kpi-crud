using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Memberships.Queries.GetActiveMembershipsByClient;

public sealed record GetActiveMembershipsByClientQuery(int ClientId) : IQuery<IReadOnlyList<MembershipDto>>;
