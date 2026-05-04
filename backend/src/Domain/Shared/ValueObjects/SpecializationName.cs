using GymManagement.Domain.Coaches.Errors;

namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class SpecializationName : ValueObject
{
    public const int MaxLength = 100;

    public string Value { get; }

    private SpecializationName(string value) => Value = value;

    public static SpecializationName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidSpecializationError("Specialization name cannot be empty.");

        if (value.Length > MaxLength)
            throw new InvalidSpecializationError($"Specialization name cannot exceed {MaxLength} characters.");

        return new SpecializationName(value.Trim());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
