using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Enrollments;

public sealed class Enrollment : Entity<int>
{
    public int ClientId { get; private set; }
    public int ClassId { get; private set; }
    public DateTimeOffset RegistrationTime { get; private set; }

    private Enrollment() { }

    private Enrollment(int id, int clientId, int classId, DateTimeOffset registrationTime)
        : base(id)
    {
        ClientId = clientId;
        ClassId = classId;
        RegistrationTime = registrationTime;
    }

    public static Enrollment Create(int clientId, int classId, DateTimeOffset registrationTime)
    {
        if (clientId <= 0)
            throw new InvalidEnrollmentError("ClientId must be a positive number.");
        if (classId <= 0)
            throw new InvalidEnrollmentError("ClassId must be a positive number.");

        return new Enrollment(0, clientId, classId, registrationTime);
    }

    public static Enrollment Reconstitute(int id, int clientId, int classId,
        DateTimeOffset registrationTime)
    {
        return new Enrollment(id, clientId, classId, registrationTime);
    }
}
