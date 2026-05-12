using FluentAssertions;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Tests.Unit.Domain;

public class TimeRangeTests
{

    [Fact]
    public void Create_ValidRange_ReturnsTimeRange()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(2);

        var range = TimeRange.Create(start, end);

        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Fact]
    public void Create_StartAfterEnd_ThrowsInvalidTimeRangeError()
    {
        var start = DateTimeOffset.UtcNow.AddHours(5);
        var end = start.AddHours(-1);

        var act = () => TimeRange.Create(start, end);
        act.Should().Throw<InvalidTimeRangeError>();
    }

    [Fact]
    public void Create_StartEqualsEnd_ThrowsInvalidTimeRangeError()
    {
        var time = DateTimeOffset.UtcNow.AddHours(1);

        var act = () => TimeRange.Create(time, time);
        act.Should().Throw<InvalidTimeRangeError>();
    }

    [Fact]
    public void Create_DurationExceeds24Hours_ThrowsInvalidTimeRangeError()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(25);

        var act = () => TimeRange.Create(start, end);
        act.Should().Throw<InvalidTimeRangeError>();
    }


    [Fact]
    public void OverlapsWith_OverlappingRanges_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var a = TimeRange.Create(now, now.AddHours(3));
        var b = TimeRange.Create(now.AddHours(2), now.AddHours(5));

        a.OverlapsWith(b).Should().BeTrue();
        b.OverlapsWith(a).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_FullyContainedRange_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var outer = TimeRange.Create(now, now.AddHours(6));
        var inner = TimeRange.Create(now.AddHours(1), now.AddHours(3));

        outer.OverlapsWith(inner).Should().BeTrue();
        inner.OverlapsWith(outer).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_NonOverlapping_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;
        var a = TimeRange.Create(now, now.AddHours(2));
        var b = TimeRange.Create(now.AddHours(3), now.AddHours(5));

        a.OverlapsWith(b).Should().BeFalse();
        b.OverlapsWith(a).Should().BeFalse();
    }

    [Fact]
    public void OverlapsWith_AdjacentRanges_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;
        var a = TimeRange.Create(now, now.AddHours(2));
        var b = TimeRange.Create(now.AddHours(2), now.AddHours(4));

        a.OverlapsWith(b).Should().BeFalse();
    }


    [Fact]
    public void IsInPast_PastRange_ReturnsTrue()
    {
        var pastStart = DateTimeOffset.UtcNow.AddDays(-2);
        var pastEnd = pastStart.AddHours(1);
        var range = TimeRange.Create(pastStart, pastEnd);

        range.IsInPast(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsInPast_FutureRange_ReturnsFalse()
    {
        var futureStart = DateTimeOffset.UtcNow.AddDays(1);
        var futureEnd = futureStart.AddHours(2);
        var range = TimeRange.Create(futureStart, futureEnd);

        range.IsInPast(DateTimeOffset.UtcNow).Should().BeFalse();
    }


    [Fact]
    public void Equals_SameStartAndEnd_AreEqual()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(2);

        var a = TimeRange.Create(start, end);
        var b = TimeRange.Create(start, end);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_AreNotEqual()
    {
        var now = DateTimeOffset.UtcNow;
        var a = TimeRange.Create(now, now.AddHours(1));
        var b = TimeRange.Create(now, now.AddHours(2));

        a.Should().NotBe(b);
    }
}
