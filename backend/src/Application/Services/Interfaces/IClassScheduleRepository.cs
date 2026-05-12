using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IClassScheduleRepository
{
    Task<GymClassDetails?> GetClassByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymClassDetails>> GetClassesByCoachAsync(int coachId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<CoachEfficiencyRow>> GetCoachEfficiencyAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
