using FluentAssertions;
using GymManagement.Services;
using GymManagement.Models;
using GymManagement.Configuration.Exceptions;
using GymManagement.Repositories.Interfaces;
using Moq;
using Xunit;

namespace GymManagement.Tests.Unit.Services;

public class ClassServiceTests
{
    private readonly Mock<IClassRepository> _classRepoMock;
    private readonly Mock<ICoachRepository> _coachRepoMock;
    private readonly Mock<IClassTypeRepository> _classTypeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClassService _service;

    public ClassServiceTests()
    {
        _classRepoMock = new Mock<IClassRepository>();
        _coachRepoMock = new Mock<ICoachRepository>();
        _classTypeRepoMock = new Mock<IClassTypeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new ClassService(
            _classRepoMock.Object,
            _coachRepoMock.Object,
            _classTypeRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateClass_ValidData_Should_ReturnClass()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(repo => repo.GetByIdAsync(classTypeId)).ReturnsAsync(new ClassType());
        _coachRepoMock.Setup(repo => repo.GetByIdAsync(coachId)).ReturnsAsync(new Coach());
        _classRepoMock.Setup(repo => repo.HasTimeConflictForCoachAsync(coachId, startTime, endTime, null)).ReturnsAsync(false);

        var result = await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        result.Should().NotBeNull();
        result.CoachId.Should().Be(coachId);
        
        _classRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Class>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateClass_StartTimeInPast_Should_ThrowException()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-10);
        var endTime = DateTime.UtcNow.AddHours(1);

        _classTypeRepoMock.Setup(repo => repo.GetByIdAsync(classTypeId)).ReturnsAsync(new ClassType());
        _coachRepoMock.Setup(repo => repo.GetByIdAsync(coachId)).ReturnsAsync(new Coach());

        var act = async () => await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Cannot schedule a class in the past.");
                 
        _classRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Class>()), Times.Never);
    }

    [Fact]
    public async Task CreateClass_CoachHasConflict_Should_ThrowException()
    {
        var classTypeId = 1;
        var coachId = 1;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        _classTypeRepoMock.Setup(repo => repo.GetByIdAsync(classTypeId)).ReturnsAsync(new ClassType());
        _coachRepoMock.Setup(repo => repo.GetByIdAsync(coachId)).ReturnsAsync(new Coach { Name = "John Doe" });
        
        _classRepoMock.Setup(repo => repo.HasTimeConflictForCoachAsync(coachId, startTime, endTime, null)).ReturnsAsync(true);

        var act = async () => await _service.CreateClassAsync(classTypeId, coachId, startTime, endTime, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Coach John Doe already has a class scheduled during this time.");
    }
    
    [Fact]
    public async Task DeleteClass_WithEnrollments_Should_ThrowException()
    {
        var classId = 1;
        var classEntity = new Class 
        { 
            ClassId = classId,
            Enrollments = new List<Enrollment> { new Enrollment() }
        };

        _classRepoMock.Setup(repo => repo.GetByIdAsync(classId)).ReturnsAsync(classEntity);

        var act = async () => await _service.DeleteClassAsync(classId);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Cannot delete a class with enrolled clients.");
    }
    [Fact]
    public async Task UpdateClass_ValidData_Should_UpdateAndSave()
    {
        var classId = 1;
        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(1);
        
        var existingClass = new Class { ClassId = classId, CoachId = 1, StartTime = DateTime.UtcNow.AddDays(1) };
        _classRepoMock.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
        _classRepoMock.Setup(r => r.HasTimeConflictForCoachAsync(1, newStart, newEnd, classId)).ReturnsAsync(false);

        var result = await _service.UpdateClassAsync(classId, newStart, newEnd);

        result.Should().NotBeNull();
        result!.StartTime.Should().Be(newStart);
        _classRepoMock.Verify(r => r.UpdateAsync(existingClass), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateClass_AlreadyStarted_Should_ThrowException()
    {
        var classId = 1;
        var existingClass = new Class { ClassId = classId, StartTime = DateTime.UtcNow.AddMinutes(-30) };
        _classRepoMock.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);

        var act = async () => await _service.UpdateClassAsync(classId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Cannot update a class that has already started.");
    }

    [Fact]
    public async Task DeleteClass_ValidNoEnrollments_Should_DeleteAndSave()
    {
        var classId = 1;
        var classEntity = new Class { ClassId = classId, Enrollments = new List<Enrollment>() };
        _classRepoMock.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(classEntity);

        var result = await _service.DeleteClassAsync(classId);

        result.Should().BeTrue();
        _classRepoMock.Verify(r => r.DeleteAsync(classId), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetCoachWorkload_ValidData_Should_CalculateCorrectly()
    {
        var coachId = 1;
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(7);
        
        _coachRepoMock.Setup(r => r.GetByIdAsync(coachId)).ReturnsAsync(new Coach { Name = "Іван" });
        
        var classes = new List<Class>
        {
            new Class { StartTime = startDate, EndTime = startDate.AddHours(2), Enrollments = new List<Enrollment> { new Enrollment(), new Enrollment() } },
            new Class { StartTime = startDate.AddDays(1), EndTime = startDate.AddDays(1).AddHours(1), Enrollments = new List<Enrollment> { new Enrollment(), new Enrollment(), new Enrollment(), new Enrollment() } }
        };
        _classRepoMock.Setup(r => r.GetClassesByCoachAsync(coachId, startDate, endDate)).ReturnsAsync(classes);

        var result = await _service.GetCoachWorkloadAsync(coachId, startDate, endDate);

        result.TotalClassesScheduled.Should().Be(2);
        result.TotalHoursWorked.Should().Be(3);
        result.AverageClassSize.Should().Be(3);
    }
}