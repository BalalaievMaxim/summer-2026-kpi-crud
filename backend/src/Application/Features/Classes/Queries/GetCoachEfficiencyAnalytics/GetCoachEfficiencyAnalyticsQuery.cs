using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Classes.Queries.GetCoachEfficiencyAnalytics;

public sealed record GetCoachEfficiencyAnalyticsQuery(DateTime StartDate, DateTime EndDate)
    : IQuery<List<CoachEfficiencyRow>>;
