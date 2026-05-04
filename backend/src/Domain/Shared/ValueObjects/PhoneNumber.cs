using System.Text.RegularExpressions;
using GymManagement.Domain.Clients.Errors;

namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex =
        new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !PhoneRegex.IsMatch(value.Trim()))
            throw new InvalidPhoneNumberError(value);

        return new PhoneNumber(value.Trim());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
