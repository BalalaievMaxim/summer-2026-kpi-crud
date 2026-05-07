using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Classes;

public class ClassFactory
{
    private readonly IClassRepository _classRepo;
    private readonly ICoachRepository _coachRepo;

    public ClassFactory(IClassRepository classRepo, ICoachRepository coachRepo)
    {
        _classRepo = classRepo;
        _coachRepo = coachRepo;
    }

    /// <summary>
    /// Creates a new Class aggregate after validating all invariants:
    /// - TimeRange is valid and not in the past
    /// - Capacity is positive
    /// - Coach exists
    /// - Coach has no schedule conflicts
    /// </summary>
    public async Task<Class> CreateAsync(
        int classTypeId,
        int coachId,
        DateTimeOffset start,
        DateTimeOffset end,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        // Simple invariants
        if (capacity <= 0)
            throw new InvalidCapacityError();

        var schedule = TimeRange.Create(start, end);

        if (schedule.IsInPast())
            throw new ClassInPastError();

        // Complex invariants (require repository)
        var coach = await _coachRepo.GetByIdAsync(coachId, cancellationToken);
        if (coach is null)
            throw new CoachNotFoundForClassError(coachId);

        var hasConflict = await _classRepo.HasOverlappingClass(coachId, schedule, null, cancellationToken);
        if (hasConflict)
            throw new CoachScheduleConflictError(coachId);

        return Class.Create(0, classTypeId, coachId, start, end, capacity);
    }
}
