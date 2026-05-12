using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class DateRange : ValueObject
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (start >= end)
            throw new DomainValidationError("DateRange.InvalidRange", "Start date must be before end date.");
        return new DateRange(start, end);
    }

    public static DateRange CreateFromMonths(DateOnly start, int months)
    {
        if (months <= 0)
            throw new DomainValidationError("DateRange.InvalidDuration", "Duration must be positive.");
        var end = start.AddMonths(months);
        return new DateRange(start, end);
    }

    public bool IsActive(DateOnly today) => today >= Start && today <= End;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:d} — {End:d}";
}
