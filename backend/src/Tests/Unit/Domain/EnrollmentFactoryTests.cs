using FluentAssertions;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;

namespace GymManagement.Tests.Unit.Domain;

public class EnrollmentFactoryTests
{
    private readonly Mock<IClassRepository> _classRepoMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly EnrollmentFactory _factory;

    private static readonly DateTimeOffset FutureStart = DateTimeOffset.UtcNow.AddDays(1);
    private static readonly DateTimeOffset FutureEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2);

    public EnrollmentFactoryTests()
    {
        _factory = new EnrollmentFactory(_classRepoMock.Object, _enrollmentRepoMock.Object);
    }

    private Class CreateClassWithCapacity(int capacity, int enrolledCount = 0)
    {
        var cls = Class.Create(1, classTypeId: 1, coachId: 1, FutureStart, FutureEnd, capacity);
        for (int i = 1; i <= enrolledCount; i++)
        {
            cls.Enroll(clientId: 1000 + i); 
        }
        return cls;
    }

    private void SetupClassExists(Class cls)
    {
        _classRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cls);
    }

    private void SetupClientNotEnrolled()
    {
        _enrollmentRepoMock
            .Setup(r => r.IsClientEnrolledAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupClientAlreadyEnrolled()
    {
        _enrollmentRepoMock
            .Setup(r => r.IsClientEnrolledAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }


    [Fact]
    public async Task CreateAsync_ValidData_ReturnsEnrollment()
    {
        var cls = CreateClassWithCapacity(capacity: 10);
        SetupClassExists(cls);
        SetupClientNotEnrolled();

        var enrollment = await _factory.CreateAsync(clientId: 1, classId: 1);

        enrollment.Should().NotBeNull();
        enrollment.ClientId.Should().Be(1);
        enrollment.ClassId.Should().Be(1);
    }


    [Fact]
    public async Task CreateAsync_InvalidClientId_ThrowsInvalidEnrollmentError()
    {
        var act = () => _factory.CreateAsync(clientId: 0, classId: 1);
        await act.Should().ThrowAsync<InvalidEnrollmentError>();
    }

    [Fact]
    public async Task CreateAsync_InvalidClassId_ThrowsInvalidEnrollmentError()
    {
        var act = () => _factory.CreateAsync(clientId: 1, classId: -1);
        await act.Should().ThrowAsync<InvalidEnrollmentError>();
    }


    [Fact]
    public async Task CreateAsync_ClassNotFound_ThrowsClassNotFoundError()
    {
        _classRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Class?)null);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 999);
        await act.Should().ThrowAsync<ClassNotFoundError>();
    }

    [Fact]
    public async Task CreateAsync_ClassFull_ThrowsClassFullError()
    {
        var cls = CreateClassWithCapacity(capacity: 1, enrolledCount: 1);
        SetupClassExists(cls);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 1);
        await act.Should().ThrowAsync<ClassFullError>();
    }

    [Fact]
    public async Task CreateAsync_AlreadyEnrolled_ThrowsClientAlreadyEnrolledError()
    {
        var cls = CreateClassWithCapacity(capacity: 10);
        SetupClassExists(cls);
        SetupClientAlreadyEnrolled();

        var act = () => _factory.CreateAsync(clientId: 1, classId: 1);
        await act.Should().ThrowAsync<ClientAlreadyEnrolledError>();
    }
}
