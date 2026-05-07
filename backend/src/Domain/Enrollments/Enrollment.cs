using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Enrollments;

public enum EnrollmentStatus
{
    Active,
    Cancelled
}

public sealed class Enrollment : Entity<int>
{
    public int ClientId { get; private set; }
    public int ClassId { get; private set; }
    public DateTimeOffset RegistrationTime { get; private set; }
    public EnrollmentStatus Status { get; private set; }

    private Enrollment() { }

    private Enrollment(int id, int clientId, int classId, DateTimeOffset registrationTime, EnrollmentStatus status)
        : base(id)
    {
        ClientId = clientId;
        ClassId = classId;
        RegistrationTime = registrationTime;
        Status = status;
    }

    public static Enrollment Create(int clientId, int classId)
    {
        if (clientId <= 0)
            throw new InvalidEnrollmentError("ClientId must be a positive number.");
        if (classId <= 0)
            throw new InvalidEnrollmentError("ClassId must be a positive number.");

        return new Enrollment(0, clientId, classId, DateTimeOffset.UtcNow, EnrollmentStatus.Active);
    }

    public void Cancel()
    {
        if (Status == EnrollmentStatus.Cancelled)
            throw new EnrollmentAlreadyCancelledError(Id);

        Status = EnrollmentStatus.Cancelled;
    }

    public bool IsActive => Status == EnrollmentStatus.Active;

    internal static Enrollment Reconstitute(int id, int clientId, int classId,
        DateTimeOffset registrationTime, EnrollmentStatus status)
    {
        return new Enrollment(id, clientId, classId, registrationTime, status);
    }
}
