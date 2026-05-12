using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Classes.Queries.GetClassAttendanceAnalytics;

public sealed record GetClassAttendanceAnalyticsQuery(DateTime StartDate, DateTime EndDate)
    : IQuery<IReadOnlyList<ClassAttendanceRow>>;
