using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;

namespace GymManagement.Application.Features.Classes.Queries.GetClassAttendanceAnalytics;

public sealed class GetClassAttendanceAnalyticsQueryHandler(IClassScheduleRepository classScheduleRepository)
    : IQueryHandler<GetClassAttendanceAnalyticsQuery, IReadOnlyList<ClassAttendanceRow>>
{
    public async Task<IReadOnlyList<ClassAttendanceRow>> Handle(
        GetClassAttendanceAnalyticsQuery query,
        CancellationToken cancellationToken = default)
    {
        var classes = await classScheduleRepository.GetScheduleForDateRangeAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);

        return classes
            .Select(c => new ClassAttendanceRow(
                c.ClassId,
                c.ClassTypeName,
                c.CoachName,
                c.StartTimeUtc,
                c.Capacity,
                c.EnrollmentClientIds.Count,
                c.Capacity > 0
                    ? (decimal)c.EnrollmentClientIds.Count / c.Capacity * 100
                    : 0))
            .OrderByDescending(row => row.OccupancyRate)
            .ToList();
    }
}
