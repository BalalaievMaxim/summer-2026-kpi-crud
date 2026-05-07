namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class PersonName : ValueObject
{
    public const int MaxLength = 100;

    public string Value { get; }

    private PersonName(string value) => Value = value;

    public static PersonName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationError("Shared.InvalidPersonName", "Name cannot be empty.");

        if (value.Trim().Length > MaxLength)
            throw new DomainValidationError("Shared.InvalidPersonName", $"Name cannot exceed {MaxLength} characters.");

        return new PersonName(value.Trim());
    }

    public static PersonName Reconstitute(string stored) => new(stored);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
