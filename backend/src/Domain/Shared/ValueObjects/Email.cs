using System.Text.RegularExpressions;
using GymManagement.Domain.Clients.Errors;

namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !EmailRegex.IsMatch(value))
            throw new InvalidEmailError(value);

        return new Email(value.Trim().ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
