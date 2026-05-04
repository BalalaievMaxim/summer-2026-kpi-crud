using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Coaches.Errors;

public sealed class InvalidSpecializationError : DomainError
{
    public InvalidSpecializationError(string message)
        : base("Coach.InvalidSpecialization", message) { }
}

public sealed class CoachNotFoundError : DomainError
{
    public CoachNotFoundError(int id)
        : base("Coach.NotFound", $"Coach with id {id} was not found.") { }
}
