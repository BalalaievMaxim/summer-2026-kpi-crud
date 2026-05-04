namespace GymManagement.Domain.Shared;

public abstract class DomainError : Exception
{
    public string Code { get; }

    protected DomainError(string code, string message) : base(message)
    {
        Code = code;
    }
}
