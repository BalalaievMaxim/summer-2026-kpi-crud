using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;
using DomainCoach = GymManagement.Domain.Coaches.Coach;
using DomainClass = GymManagement.Domain.Classes.Class;

namespace GymManagement.Tests.Unit.Services;

public sealed class ClassServiceTests
{
    private readonly Mock<IClassRepositoryPort> _classRepoMock;
    private readonly Mock<IClassScheduleRepository> _classScheduleRepoMock;
    private readonly Mock<ICoachRepository> _coachRepoMock;
    private readonly Mock<IClassTypeRepositoryPort> _classTypeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClassService _service;

    public ClassServiceTests()
    {
        _classRepoMock = new Mock<IClassRepositoryPort>();
        _classScheduleRepoMock = new Mock<IClassScheduleRepository>();
        _coachRepoMock = new Mock<ICoachRepository>();
        _classTypeRepoMock = new Mock<IClassTypeRepositoryPort>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var factory = new ClassFactory(_classRepoMock.Object, _coachRepoMock.Object);

        _service = new ClassService(
            _classRepoMock.Object,
            _classScheduleRepoMock.Object,
            _coachRepoMock.Object,
            _classTypeRepoMock.Object,
            factory,
            _unitOfWorkMock.Object);
    }

    private static GymClassDetails Details(int id, int coachId, DateTime start, DateTime end, int capacity, params int[] enrollmentIds)
        => new(id, 1, "type", coachId, "coach", start, end, capacity, enrollmentIds);

    private static DomainClass ClassAggregate(int id, int coachId, DateTime start, DateTime end, int capacity, params int[] enrollmentClientIds)
    {
        var schedule = TimeRange.Create(
            new DateTimeOffset(DateTime.SpecifyKind(start, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(end, DateTimeKind.Utc)));

        var enrollments = enrollmentClientIds.Select((clientId, index) =>
            GymManagement.Domain.Enrollments.Enrollment.Reconstitute(
                index + 1,
                clientId,
                id,
                DateTimeOffset.UtcNow));

        return DomainClass.Reconstitute(id, 1, coachId, schedule, capacity, enrollments);
    }

    private void SetupCoach(int coachId, string name = "Test Coach")
        => _coachRepoMock.Setup(r => r.GetByIdAsync(coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainCoach.Reconstitute(coachId, name, "coach@test.com", "Yoga", "pass1234"));

    [Fact]
    public async Task CreateClass_ValidData_Should_ReturnClass()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(r => r.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupCoach(coachId);
        _classRepoMock.Setup(r => r.HasOverlappingClassAsync(coachId, It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var expected = Details(99, coachId, startTime, endTime, 10);
        _classRepoMock.Setup(r => r.AddAsync(It.IsAny<DomainClass>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);
        _classScheduleRepoMock.Setup(r => r.GetClassByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        result.Should().Be(expected);
        _classRepoMock.Verify(r => r.AddAsync(It.Is<DomainClass>(c =>
            c.ClassTypeId == classTypeId &&
            c.CoachId == coachId &&
            c.Capacity == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClass_StartTimeInPast_Should_ThrowDomainError()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-10);
        var endTime = DateTime.UtcNow.AddHours(1);

        _classTypeRepoMock.Setup(r => r.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupCoach(coachId);

        var act = async () => await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        await act.Should().ThrowAsync<ClassInPastError>();

        _classRepoMock.Verify(r => r.AddAsync(It.IsAny<DomainClass>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClass_CoachHasConflict_Should_ThrowDomainError()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(r => r.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupCoach(coachId, "John Doe");
        _classRepoMock.Setup(r => r.HasOverlappingClassAsync(coachId, It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        await act.Should().ThrowAsync<CoachScheduleConflictError>();
    }

    [Fact]
    public async Task DeleteClass_WithEnrollments_Should_ThrowDomainError()
    {
        var classId = 1;
        var session = ClassAggregate(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10, 5);

        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var act = async () => await _service.DeleteClassAsync(classId);

        await act.Should().ThrowAsync<ClassHasEnrollmentsError>();
    }

    [Fact]
    public async Task UpdateClass_ValidData_Should_Update()
    {
        var classId = 1;
        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(1);

        var existing = ClassAggregate(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10);
        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _classRepoMock.Setup(r => r.HasOverlappingClassAsync(1, It.IsAny<TimeRange>(), classId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var updated = Details(classId, 1, newStart, newEnd, 10);
        _classRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DomainClass>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _classScheduleRepoMock.Setup(r => r.GetClassByIdAsync(classId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _service.UpdateClassAsync(classId, newStart, newEnd);

        result.Should().NotBeNull();
        result!.StartTimeUtc.Should().Be(newStart);
        _classRepoMock.Verify(r => r.UpdateAsync(It.Is<DomainClass>(c => c.Id == classId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateClass_AlreadyStarted_Should_ThrowDomainError()
    {
        var classId = 1;
        var existing = ClassAggregate(classId, 1, DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-10), 10);
        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = async () => await _service.UpdateClassAsync(classId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        await act.Should().ThrowAsync<ClassInPastError>();
    }

    [Fact]
    public async Task DeleteClass_ValidNoEnrollments_Should_DeleteAndSave()
    {
        var classId = 1;
        var session = ClassAggregate(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10);
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

        SetupCoach(coachId, "Іван");

        var classes = new List<GymClassDetails>
        {
            Details(1, coachId, startDate, startDate.AddHours(2), 10, 1, 2),
            Details(2, coachId, startDate.AddDays(1), startDate.AddDays(1).AddHours(1), 10, 1, 2, 3, 4)
        };
        _classScheduleRepoMock.Setup(r => r.GetClassesByCoachAsync(coachId, startDate, endDate, It.IsAny<CancellationToken>())).ReturnsAsync(classes);

        var result = await _service.GetCoachWorkloadAsync(coachId, startDate, endDate);

        result.TotalClassesScheduled.Should().Be(2);
        result.TotalHoursWorked.Should().Be(3);
        result.AverageClassSize.Should().Be(3);
    }
}
