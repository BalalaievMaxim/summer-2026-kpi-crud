using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Classes;

public sealed class Class : AggregateRoot<int>
{
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
        if (capacity <= 0)
            throw new InvalidCapacityError();

        var schedule = TimeRange.Create(start, end);
        return new Class(id, classTypeId, coachId, schedule, capacity);
    }

    /// <summary>
    /// Enroll a client into this class.
    /// Checks: class not in past, has capacity, no duplicate enrollment.
    /// </summary>
    public Enrollment Enroll(int clientId)
    {
        if (Schedule.IsInPast())
            throw new ClassInPastError();

        if (IsFull)
            throw new ClassFullError(Id);

        if (_enrollments.Any(e => e.ClientId == clientId))
            throw new DuplicateEnrollmentError(clientId, Id);

        var enrollment = Enrollment.Create(clientId, Id);
        _enrollments.Add(enrollment);
        return enrollment;
    }

    /// <summary>
    /// Cancel enrollment for a specific client.
    /// </summary>
    public void CancelEnrollment(int clientId)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.ClientId == clientId);
        if (enrollment is null)
            throw new EnrollmentNotFoundInClassError(clientId, Id);

        _enrollments.Remove(enrollment);
    }

    /// <summary>
    /// Check if there is capacity for a given number of additional enrollments.
    /// </summary>
    public bool HasCapacityFor(int count)
        => _enrollments.Count + count <= Capacity;

    /// <summary>
    /// Reschedule the class to a new time range.
    /// The new range must not be in the past.
    /// </summary>
    public void Reschedule(TimeRange newRange)
    {
        if (newRange.IsInPast())
            throw new ClassInPastError();

        Schedule = newRange;
    }

    /// <summary>
    /// Used by infrastructure layer to reconstitute the aggregate from persistence.
    /// </summary>
    internal static Class Reconstitute(int id, int classTypeId, int coachId,
        TimeRange schedule, int capacity, IEnumerable<Enrollment> enrollments)
    {
        var cls = new Class(id, classTypeId, coachId, schedule, capacity);
        cls._enrollments.AddRange(enrollments);
        return cls;
    }
}
