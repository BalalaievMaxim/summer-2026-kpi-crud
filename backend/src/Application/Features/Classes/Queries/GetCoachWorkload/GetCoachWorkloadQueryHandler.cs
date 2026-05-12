using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Classes.Queries.GetCoachWorkload;

public sealed class GetCoachWorkloadQueryHandler(
    IClassScheduleRepository classScheduleRepository,
    ICoachReadRepository coachReadRepository) : IQueryHandler<GetCoachWorkloadQuery, CoachWorkloadRow>
{
    public async Task<CoachWorkloadRow> Handle(GetCoachWorkloadQuery query, CancellationToken cancellationToken = default)
    {
        var coach = await coachReadRepository.GetByIdAsync(query.CoachId, cancellationToken)
            ?? throw new NotFoundException($"Coach with ID {query.CoachId} not found.");

        var classes = await classScheduleRepository.GetClassesByCoachAsync(
            query.CoachId,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        var list = classes.ToList();
        var totalHours = list.Sum(c => (c.EndTimeUtc - c.StartTimeUtc).TotalHours);
        var averageSize = list.Count != 0
            ? (int)list.Average(c => c.EnrollmentClientIds.Count)
            : 0;

        return new CoachWorkloadRow(
            query.CoachId,
            coach.Name,
            list.Count,
            (int)totalHours,
            averageSize);
    }
}