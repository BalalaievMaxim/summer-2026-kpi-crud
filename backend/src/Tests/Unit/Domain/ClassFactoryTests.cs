using FluentAssertions;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;

namespace GymManagement.Tests.Unit.Domain;

public class ClassFactoryTests
{
    private readonly Mock<IClassRepository> _classRepoMock = new();
    private readonly Mock<ICoachRepository> _coachRepoMock = new();
    private readonly ClassFactory _factory;

    private static readonly DateTimeOffset FutureStart = DateTimeOffset.UtcNow.AddDays(1);
    private static readonly DateTimeOffset FutureEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2);

    public ClassFactoryTests()
    {
        _factory = new ClassFactory(_classRepoMock.Object, _coachRepoMock.Object);
    }

    private void SetupCoachExists(int coachId = 1)
    {
        _coachRepoMock
            .Setup(r => r.GetByIdAsync(coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Coach.Create(coachId, "Test", "Coach", "Yoga", 1));
    }

    private void SetupNoScheduleConflict()
    {
        _classRepoMock
            .Setup(r => r.HasOverlappingClass(
                It.IsAny<int>(), It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupScheduleConflict()
    {
        _classRepoMock
            .Setup(r => r.HasOverlappingClass(
                It.IsAny<int>(), It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }


    [Fact]
    public async Task CreateAsync_ValidData_ReturnsClass()
    {
        SetupCoachExists();
        SetupNoScheduleConflict();

        var result = await _factory.CreateAsync(
            classTypeId: 1, coachId: 1,
            start: FutureStart, end: FutureEnd, capacity: 20);

        result.Should().NotBeNull();
        result.ClassTypeId.Should().Be(1);
        result.CoachId.Should().Be(1);
        result.Capacity.Should().Be(20);
    }


    [Fact]
    public async Task CreateAsync_ZeroCapacity_ThrowsInvalidCapacityError()
    {
        var act = () => _factory.CreateAsync(1, 1, FutureStart, FutureEnd, capacity: 0);
        await act.Should().ThrowAsync<InvalidCapacityError>();
    }

    [Fact]
    public async Task CreateAsync_PastTimeRange_ThrowsClassInPastError()
    {
        var pastStart = DateTimeOffset.UtcNow.AddDays(-2);
        var pastEnd = pastStart.AddHours(1);

        var act = () => _factory.CreateAsync(1, 1, pastStart, pastEnd, capacity: 10);
        await act.Should().ThrowAsync<ClassInPastError>();
    }

    [Fact]
    public async Task CreateAsync_StartAfterEnd_ThrowsInvalidTimeRangeError()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(-1);

        var act = () => _factory.CreateAsync(1, 1, start, end, capacity: 10);
        await act.Should().ThrowAsync<InvalidTimeRangeError>();
    }


    [Fact]
    public async Task CreateAsync_CoachNotFound_ThrowsCoachNotFoundForClassError()
    {
        _coachRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Coach?)null);

        var act = () => _factory.CreateAsync(1, coachId: 99, FutureStart, FutureEnd, capacity: 10);
        await act.Should().ThrowAsync<CoachNotFoundForClassError>();
    }

    [Fact]
    public async Task CreateAsync_CoachHasConflict_ThrowsCoachScheduleConflictError()
    {
        SetupCoachExists();
        SetupScheduleConflict();

        var act = () => _factory.CreateAsync(1, 1, FutureStart, FutureEnd, capacity: 10);
        await act.Should().ThrowAsync<CoachScheduleConflictError>();
    }
}
