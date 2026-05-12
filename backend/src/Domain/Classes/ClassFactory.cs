using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Classes;

public class ClassFactory
{
    private readonly IClassRepositoryPort _classRepo;
    private readonly ICoachRepository _coachRepo;

    public ClassFactory(IClassRepositoryPort classRepo, ICoachRepository coachRepo)
    {
        _classRepo = classRepo;
        _coachRepo = coachRepo;
    }

    public async Task<Class> CreateAsync(
        int classTypeId,
        int coachId,
        DateTimeOffset start,
        DateTimeOffset end,
        int capacity,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        if (capacity <= 0 || capacity > Class.MaxCapacity)
            throw new InvalidCapacityError();

        var schedule = TimeRange.Create(start, end);

        if (schedule.IsInPast(now))
            throw new ClassInPastError();

        var coach = await _coachRepo.GetByIdAsync(coachId, cancellationToken);
        if (coach is null)
            throw new CoachNotFoundForClassError(coachId);

        var hasConflict = await _classRepo.HasOverlappingClassAsync(coachId, schedule, null, cancellationToken);
        if (hasConflict)
            throw new CoachScheduleConflictError(coachId);

        return Class.Create(0, classTypeId, coachId, start, end, capacity);
    }
}
