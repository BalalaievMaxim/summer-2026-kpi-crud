using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Classes;

public sealed class Class : AggregateRoot<int>
{
    public const int MaxCapacity = 50;

    public int ClassTypeId { get; private set; }
    public int CoachId { get; private set; }
    public TimeRange Schedule { get; private set; } = null!;
    public int Capacity { get; private set; }

    private readonly List<Enrollment> _enrollments = new();
    public IReadOnlyList<Enrollment> Enrollments => _enrollments.AsReadOnly();

    public bool IsFull => _enrollments.Count >= Capacity;

    private Class() { }

    private Class(int id, int classTypeId, int coachId, TimeRange schedule, int capacity)
        : base(id)
    {
        ClassTypeId = classTypeId;
        CoachId = coachId;
        Schedule = schedule;
        Capacity = capacity;
    }

    public static Class Create(int id, int classTypeId, int coachId,
        DateTimeOffset start, DateTimeOffset end, int capacity)
    {
        if (capacity <= 0 || capacity > MaxCapacity)
            throw new InvalidCapacityError();

        var schedule = TimeRange.Create(start, end);
        return new Class(id, classTypeId, coachId, schedule, capacity);
    }

    public Enrollment Enroll(int clientId, DateTimeOffset now)
    {
        if (Schedule.IsInPast(now))
            throw new ClassInPastError();

        if (IsFull)
            throw new ClassFullError(Id);

        if (_enrollments.Any(e => e.ClientId == clientId))
            throw new DuplicateEnrollmentError(clientId, Id);

        var enrollment = Enrollment.Create(clientId, Id, now);
        _enrollments.Add(enrollment);
        return enrollment;
    }

    public void CancelEnrollment(int clientId)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.ClientId == clientId);
        if (enrollment is null)
            throw new EnrollmentNotFoundInClassError(clientId, Id);

        _enrollments.Remove(enrollment);
    }

    public bool HasCapacityFor(int count)
        => _enrollments.Count + count <= Capacity;

    public void Reschedule(TimeRange newRange, DateTimeOffset now)
    {
        if (newRange.IsInPast(now))
            throw new ClassInPastError();

        Schedule = newRange;
    }

    public void Reschedule(TimeRange newRange, bool coachHasConflict, DateTimeOffset now)
    {
        if (coachHasConflict)
            throw new CoachScheduleConflictError(CoachId);

        Reschedule(newRange, now);
    }

    public void EnsureCanBeDeleted()
    {
        if (_enrollments.Count > 0)
            throw new ClassHasEnrollmentsError(Id);
    }

    public static Class Reconstitute(int id, int classTypeId, int coachId,
        TimeRange schedule, int capacity, IEnumerable<Enrollment> enrollments)
    {
        var cls = new Class(id, classTypeId, coachId, schedule, capacity);
        cls._enrollments.AddRange(enrollments);
        return cls;
    }
}
