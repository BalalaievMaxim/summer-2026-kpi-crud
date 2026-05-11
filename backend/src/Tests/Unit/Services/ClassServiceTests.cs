using FluentAssertions;
using GymManagement.Application.Services;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Ports;
using Moq;
using DomainCoach = GymManagement.Domain.Coaches.Coach;

namespace GymManagement.Tests.Unit.Services;

public sealed class ClassServiceTests
{
    private readonly Mock<IClassScheduleRepository> _classRepoMock;
    private readonly Mock<ICoachRepository> _coachRepoMock;
    private readonly Mock<IClassTypeRepositoryPort> _classTypeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClassService _service;

    public ClassServiceTests()
    {
        _classRepoMock = new Mock<IClassScheduleRepository>();
        _coachRepoMock = new Mock<ICoachRepository>();
        _classTypeRepoMock = new Mock<IClassTypeRepositoryPort>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new ClassService(
            _classRepoMock.Object,
            _coachRepoMock.Object,
            _classTypeRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private static GymClassDetails Details(int id, int coachId, DateTime start, DateTime end, int capacity, params int[] enrollmentIds)
        => new(id, 1, "type", coachId, "coach", start, end, capacity, enrollmentIds);

    [Fact]
    public async Task CreateClass_ValidData_Should_ReturnClass()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(repo => repo.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _coachRepoMock.Setup(repo => repo.GetByIdAsync(coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainCoach.Reconstitute(coachId, "Test Coach", "coach@test.com", "Yoga", "pass1234"));
        _classRepoMock.Setup(repo => repo.HasTimeConflictForCoachAsync(coachId, startTime, endTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var expected = Details(99, coachId, startTime, endTime, 10);
        _classRepoMock.Setup(r => r.CreateAsync(classTypeId, coachId, startTime, endTime, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        result.Should().Be(expected);
        _classRepoMock.Verify(repo => repo.CreateAsync(classTypeId, coachId, startTime, endTime, 10, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClass_StartTimeInPast_Should_ThrowException()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-10);
        var endTime = DateTime.UtcNow.AddHours(1);

        _classTypeRepoMock.Setup(repo => repo.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _coachRepoMock.Setup(repo => repo.GetByIdAsync(coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainCoach.Reconstitute(coachId, "Test Coach", "coach@test.com", "Yoga", "pass1234"));

        var act = async () => await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot schedule a class in the past.");

        _classRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClass_CoachHasConflict_Should_ThrowException()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(repo => repo.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _coachRepoMock.Setup(repo => repo.GetByIdAsync(coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainCoach.Reconstitute(coachId, "John Doe", "john@test.com", "Yoga", "pass1234"));

        _classRepoMock.Setup(repo => repo.HasTimeConflictForCoachAsync(coachId, startTime, endTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Coach John Doe already has a class scheduled during this time.");
    }

    [Fact]
    public async Task DeleteClass_WithEnrollments_Should_ThrowException()
    {
        var classId = 1;
        var session = Details(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10, 5);

        _classRepoMock.Setup(repo => repo.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var act = async () => await _service.DeleteClassAsync(classId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete a class with enrolled clients.");
    }

    [Fact]
    public async Task UpdateClass_ValidData_Should_Update()
    {
        var classId = 1;
        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(1);

        var existing = Details(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10);
        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _classRepoMock.Setup(r => r.HasTimeConflictForCoachAsync(1, newStart, newEnd, classId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var updated = Details(classId, 1, newStart, newEnd, 10);
        _classRepoMock.Setup(r => r.UpdateTimesAsync(classId, newStart, newEnd, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var result = await _service.UpdateClassAsync(classId, newStart, newEnd);

        result.Should().NotBeNull();
        result!.StartTimeUtc.Should().Be(newStart);
        _classRepoMock.Verify(r => r.UpdateTimesAsync(classId, newStart, newEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateClass_AlreadyStarted_Should_ThrowException()
    {
        var classId = 1;
        var existing = Details(classId, 1, DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-10), 10);
        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = async () => await _service.UpdateClassAsync(classId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update a class that has already started.");
    }

    [Fact]
    public async Task DeleteClass_ValidNoEnrollments_Should_DeleteAndSave()
    {
        var classId = 1;
        var session = Details(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10);
        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _classRepoMock.Setup(r => r.DeleteAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.DeleteClassAsync(classId);

        result.Should().BeTrue();
        _classRepoMock.Verify(r => r.DeleteAsync(classId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCoachWorkload_ValidData_Should_CalculateCorrectly()
    {
        var coachId = 1;
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(7);

        _coachRepoMock.Setup(r => r.GetByIdAsync(coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainCoach.Reconstitute(coachId, "Іван", "ivan@test.com", "Yoga", "pass1234"));

        var classes = new List<GymClassDetails>
        {
            Details(1, coachId, startDate, startDate.AddHours(2), 10, 1, 2),
            Details(2, coachId, startDate.AddDays(1), startDate.AddDays(1).AddHours(1), 10, 1, 2, 3, 4)
        };
        _classRepoMock.Setup(r => r.GetClassesByCoachAsync(coachId, startDate, endDate, It.IsAny<CancellationToken>())).ReturnsAsync(classes);

        var result = await _service.GetCoachWorkloadAsync(coachId, startDate, endDate);

        result.TotalClassesScheduled.Should().Be(2);
        result.TotalHoursWorked.Should().Be(3);
        result.AverageClassSize.Should().Be(3);
    }
}
