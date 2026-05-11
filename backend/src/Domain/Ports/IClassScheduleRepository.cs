using GymManagement.Domain.Classes;
using GymManagement.Domain.Queries;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Ports;

public interface IClassScheduleRepository
{
    Task<GymClassDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GymClassDetails?> GetByIdWithEnrollmentsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<GymClassDetails> CreateAsync(int classTypeId, int coachId, DateTime startUtc, DateTime endUtc, int capacity, CancellationToken cancellationToken = default);
    Task<GymClassDetails?> UpdateTimesAsync(int classId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasTimeConflictForCoachAsync(int coachId, DateTime startTime, DateTime endTime, int? excludeClassId = null, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingClassAsync(int coachId, TimeRange range, int? excludeClassId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymClassDetails>> GetClassesByCoachAsync(int coachId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<CoachEfficiencyRow>> GetCoachEfficiencyAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
