using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Features.Classes.Commands.CreateClass;
using GymManagement.Application.Features.Classes.Commands.DeleteClass;
using GymManagement.Application.Features.Classes.Commands.RescheduleClass;
using GymManagement.Application.Features.Classes.Queries.GetCoachWorkload;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;
using DomainClass = GymManagement.Domain.Classes.Class;
using DomainCoach = GymManagement.Domain.Coaches.Coach;

namespace GymManagement.Tests.Unit.Features.Classes;

public sealed class ClassHandlerTests
{
    private readonly Mock<IClassRepositoryPort> _classRepoMock = new();
    private readonly Mock<IClassScheduleRepository> _classScheduleRepoMock = new();
    private readonly Mock<ICoachRepository> _coachRepoMock = new();
    private readonly Mock<IClassTypeRepositoryPort> _classTypeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly CreateClassCommandHandler _createHandler;
    private readonly RescheduleClassCommandHandler _rescheduleHandler;
    private readonly DeleteClassCommandHandler _deleteHandler;
    private readonly GetCoachWorkloadQueryHandler _coachWorkloadHandler;

    public ClassHandlerTests()
    {
        var factory = new ClassFactory(_classRepoMock.Object, _coachRepoMock.Object);

        _createHandler = new CreateClassCommandHandler(
            _classRepoMock.Object,
            _classTypeRepoMock.Object,
            factory);

        _rescheduleHandler = new RescheduleClassCommandHandler(
            _classRepoMock.Object,
            _unitOfWorkMock.Object);

        _deleteHandler = new DeleteClassCommandHandler(
            _classRepoMock.Object,
            _unitOfWorkMock.Object);

        _coachWorkloadHandler = new GetCoachWorkloadQueryHandler(
            _classScheduleRepoMock.Object,
            _coachRepoMock.Object);
    }

    private static GymClassDetails Details(
        int id,
        int coachId,
        DateTime start,
        DateTime end,
        int capacity,
        params int[] enrollmentIds)
        => new(id, 1, "type", coachId, "coach", start, end, capacity, enrollmentIds);

    private static DomainClass ClassAggregate(
        int id,
        int coachId,
        DateTime start,
        DateTime end,
        int capacity,
        params int[] enrollmentClientIds)
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
    public async Task CreateClass_ValidData_Should_ReturnIdAndPersistAggregate()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(r => r.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupCoach(coachId);
        _classRepoMock
            .Setup(r => r.HasOverlappingClassAsync(coachId, It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _classRepoMock.Setup(r => r.AddAsync(It.IsAny<DomainClass>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);

        var result = await _createHandler.Handle(
            new CreateClassCommand(classTypeId, coachId, startTime, endTime, 10));

        result.Should().Be(99);
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

        _classTypeRepoMock.Setup(r => r.ExistsAsync(classTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupCoach(coachId);

        var act = async () => await _createHandler.Handle(new CreateClassCommand(
            classTypeId,
            coachId,
            DateTime.UtcNow.AddMinutes(-10),
            DateTime.UtcNow.AddHours(1),
            10));

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
        SetupCoach(coachId);
        _classRepoMock
            .Setup(r => r.HasOverlappingClassAsync(coachId, It.IsAny<TimeRange>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _createHandler.Handle(
            new CreateClassCommand(classTypeId, coachId, startTime, endTime, 10));

        await act.Should().ThrowAsync<CoachScheduleConflictError>();
    }

    [Fact]
    public async Task RescheduleClass_ValidData_Should_UpdateAndSave()
    {
        var classId = 1;
        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(1);
        var existing = ClassAggregate(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10);

        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _classRepoMock
            .Setup(r => r.HasOverlappingClassAsync(1, It.IsAny<TimeRange>(), classId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _rescheduleHandler.Handle(new RescheduleClassCommand(classId, newStart, newEnd));

        _classRepoMock.Verify(r => r.UpdateAsync(It.Is<DomainClass>(c =>
            c.Id == classId &&
            c.Schedule.Start.UtcDateTime == newStart), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RescheduleClass_AlreadyStarted_Should_ThrowDomainError()
    {
        var classId = 1;
        var existing = ClassAggregate(classId, 1, DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(30), 10);
        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = async () => await _rescheduleHandler.Handle(new RescheduleClassCommand(
            classId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1)));

        await act.Should().ThrowAsync<ClassInPastError>();
    }

    [Fact]
    public async Task DeleteClass_WithEnrollments_Should_ThrowDomainError()
    {
        var classId = 1;
        var session = ClassAggregate(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10, 5);

        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var act = async () => await _deleteHandler.Handle(new DeleteClassCommand(classId));

        await act.Should().ThrowAsync<ClassHasEnrollmentsError>();
    }

    [Fact]
    public async Task DeleteClass_ValidNoEnrollments_Should_DeleteAndSave()
    {
        var classId = 1;
        var session = ClassAggregate(classId, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 10);

        _classRepoMock.Setup(r => r.GetByIdAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _classRepoMock.Setup(r => r.DeleteAsync(classId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _deleteHandler.Handle(new DeleteClassCommand(classId));

        _classRepoMock.Verify(r => r.DeleteAsync(classId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCoachWorkload_ValidData_Should_CalculateCorrectly()
    {
        var coachId = 1;
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(7);

        SetupCoach(coachId, "Ivan");

        var classes = new List<GymClassDetails>
        {
            Details(1, coachId, startDate, startDate.AddHours(2), 10, 1, 2),
            Details(2, coachId, startDate.AddDays(1), startDate.AddDays(1).AddHours(1), 10, 1, 2, 3, 4)
        };
        _classScheduleRepoMock
            .Setup(r => r.GetClassesByCoachAsync(coachId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classes);

        var result = await _coachWorkloadHandler.Handle(new GetCoachWorkloadQuery(coachId, startDate, endDate));

        result.TotalClassesScheduled.Should().Be(2);
        result.TotalHoursWorked.Should().Be(3);
        result.AverageClassSize.Should().Be(3);
    }

    [Fact]
    public async Task GetCoachWorkload_MissingCoach_Should_ThrowDomainError()
    {
        var act = async () => await _coachWorkloadHandler.Handle(new GetCoachWorkloadQuery(404, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));

        await act.Should().ThrowAsync<CoachNotFoundError>();
    }
}
