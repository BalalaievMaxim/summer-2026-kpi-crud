namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class Password : ValueObject
{
    public string Value { get; }

    private Password(string value) => Value = value;

    public static Password Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Length < 4)
            throw new DomainValidationError("Shared.InvalidPassword", "Password must be at least 4 characters.");

        return new Password(BCrypt.Net.BCrypt.HashPassword(raw));
    }

    public static Password Reconstitute(string stored) => new(stored);

    public bool Matches(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (Value == raw)
            return true;

        return Value.StartsWith("$2", StringComparison.Ordinal) &&
               BCrypt.Net.BCrypt.Verify(raw, Value);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => "***";
}
