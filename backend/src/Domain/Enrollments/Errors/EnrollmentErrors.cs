using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Enrollments.Errors;

public sealed class InvalidEnrollmentError : DomainError
{
    public InvalidEnrollmentError(string message)
        : base("Enrollment.Invalid", message) { }
}

public sealed class EnrollmentNotFoundError : DomainError
{
    public EnrollmentNotFoundError(int id)
        : base("Enrollment.NotFound", $"Enrollment with id {id} was not found.") { }
}

public sealed class ClientAlreadyEnrolledError : DomainError
{
    public ClientAlreadyEnrolledError(int clientId, int classId)
        : base("Enrollment.AlreadyEnrolled", $"Client {clientId} is already enrolled in class {classId}.") { }
}

public sealed class ClientHasNoActiveMembershipError : DomainError
{
    public ClientHasNoActiveMembershipError(int clientId)
        : base("Enrollment.NoActiveMembership", $"Client {clientId} does not have an active membership.") { }
}

