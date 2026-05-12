using GymManagement.Domain.Classes.Errors;

namespace GymManagement.Domain.Shared.ValueObjects;

public sealed class TimeRange : ValueObject
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    private TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    public static TimeRange Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (start >= end)
            throw new InvalidTimeRangeError("Start time must be before end time.");

        var duration = end - start;
        if (duration > TimeSpan.FromHours(24))
            throw new InvalidTimeRangeError("Time range cannot exceed 24 hours.");

        return new TimeRange(start, end);
    }

    public bool OverlapsWith(TimeRange other)
        => Start < other.End && other.Start < End;

    public bool IsInPast(DateTimeOffset now)
        => Start < now;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:g} — {End:g}";
}
