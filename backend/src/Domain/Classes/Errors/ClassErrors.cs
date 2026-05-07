using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Classes.Errors;

public sealed class InvalidTimeRangeError : DomainError
{
    public InvalidTimeRangeError(string message)
        : base("Class.InvalidTimeRange", message) { }
}

public sealed class ClassNotFoundError : DomainError
{
    public ClassNotFoundError(int id)
        : base("Class.NotFound", $"Class with id {id} was not found.") { }
}

public sealed class ClassFullError : DomainError
{
    public ClassFullError(int classId)
        : base("Class.Full", $"Class {classId} has reached maximum capacity.") { }
}

public sealed class ClassInPastError : DomainError
{
    public ClassInPastError()
        : base("Class.InPast", "Cannot perform this operation on a class scheduled in the past.") { }
}

public sealed class DuplicateEnrollmentError : DomainError
{
    public DuplicateEnrollmentError(int clientId, int classId)
        : base("Class.DuplicateEnrollment", $"Client {clientId} is already enrolled in class {classId}.") { }
}

public sealed class InvalidClassTypeError : DomainError
{
    public InvalidClassTypeError(string message)
        : base("ClassType.Invalid", message) { }
}

public sealed class CoachScheduleConflictError : DomainError
{
    public CoachScheduleConflictError(int coachId)
        : base("Class.CoachConflict", $"Coach {coachId} already has a class scheduled in this time range.") { }
}

public sealed class InvalidCapacityError : DomainError
{
    public InvalidCapacityError()
        : base("Class.InvalidCapacity", "Capacity must be greater than zero.") { }
}

public sealed class EnrollmentNotFoundInClassError : DomainError
{
    public EnrollmentNotFoundInClassError(int clientId, int classId)
        : base("Class.EnrollmentNotFound", $"Client {clientId} is not enrolled in class {classId}.") { }
}

public sealed class CoachNotFoundForClassError : DomainError
{
    public CoachNotFoundForClassError(int coachId)
        : base("Class.CoachNotFound", $"Coach with id {coachId} was not found.") { }
}
