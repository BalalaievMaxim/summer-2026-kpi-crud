using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "UAH")
    {
        if (amount < 0)
            throw new DomainValidationError("Money.NegativeAmount", "Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationError("Money.EmptyCurrency", "Currency cannot be empty.");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainValidationError("Money.CurrencyMismatch", $"Cannot add amounts with different currencies: {Currency} and {other.Currency}");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainValidationError("Money.CurrencyMismatch", $"Cannot subtract amounts with different currencies: {Currency} and {other.Currency}");
        if (Amount < other.Amount)
            throw new DomainValidationError("Money.NegativeResult", "Result cannot be negative.");
        return new Money(Amount - other.Amount, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainValidationError("Money.CurrencyMismatch", "Cannot compare amounts with different currencies.");
        return Amount > other.Amount;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
