using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Classes.Queries.GetClassById;

public sealed record GetClassByIdQuery(int ClassId) : IQuery<GymClassDetails?>;
