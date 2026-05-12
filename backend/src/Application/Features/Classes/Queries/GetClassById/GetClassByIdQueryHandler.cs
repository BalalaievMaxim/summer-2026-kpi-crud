using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Classes.Queries.GetClassById;

public sealed class GetClassByIdQueryHandler(IClassScheduleRepository classScheduleRepository)
    : IQueryHandler<GetClassByIdQuery, GymClassDetails?>
{
    public Task<GymClassDetails?> Handle(GetClassByIdQuery query, CancellationToken cancellationToken = default)
        => classScheduleRepository.GetClassByIdAsync(query.ClassId, cancellationToken);
}
