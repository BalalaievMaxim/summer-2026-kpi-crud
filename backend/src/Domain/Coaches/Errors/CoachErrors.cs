using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Coaches.Errors;

public sealed class InvalidSpecializationError : DomainError
{
    public InvalidSpecializationError(string message)
        : base("Coach.InvalidSpecialization", message) { }
}

public sealed class InvalidCoachNameError : DomainError
{
    public InvalidCoachNameError()
        : base("Coach.InvalidName", "Coach name cannot be empty.") { }
}

public sealed class InvalidPasswordError : DomainError
{
    public InvalidPasswordError()
        : base("Coach.InvalidPassword", "Password must be at least 4 characters.") { }
}

public sealed class InvalidCoachCredentialsError : DomainError
{
    public InvalidCoachCredentialsError()
        : base("Coach.InvalidCredentials", "Invalid email or password.") { }
}

public sealed class CoachEmailAlreadyExistsError : DomainError
{
    public CoachEmailAlreadyExistsError(string email)
        : base("Coach.EmailAlreadyExists", $"A coach with email '{email}' already exists.") { }
}

public sealed class CoachHasFutureClassesError : DomainError
{
    public CoachHasFutureClassesError(int coachId)
        : base("Coach.HasFutureClasses", $"Coach {coachId} has upcoming classes with enrolled clients. Cancel enrollments first.") { }
}

public sealed class CoachNotFoundError : DomainError
{
    public CoachNotFoundError(int id)
        : base("Coach.NotFound", $"Coach with id {id} was not found.") { }
}
