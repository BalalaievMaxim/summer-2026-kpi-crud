namespace GymManagement.Domain.Shared;

public sealed class DomainValidationError : DomainError
{
    public DomainValidationError(string code, string message) : base(code, message) { }
}
