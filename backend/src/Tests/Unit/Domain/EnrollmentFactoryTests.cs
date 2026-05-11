using FluentAssertions;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Ports;
using Moq;

namespace GymManagement.Tests.Unit.Domain;

public sealed class EnrollmentFactoryTests
{
    private readonly Mock<IClassScheduleRepository> _classRepoMock = new();
    private readonly Mock<IEnrollmentRepositoryPort> _enrollmentRepoMock = new();
    private readonly EnrollmentFactory _factory;

    private static GymClassDetails Session(int classId, int capacity, params int[] enrollmentClients)
        => new(classId, 1, "t", 1, "c", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), capacity, enrollmentClients);

    public EnrollmentFactoryTests()
    {
        _factory = new EnrollmentFactory(_classRepoMock.Object, _enrollmentRepoMock.Object);
    }

    private void SetupClassExists(GymClassDetails details)
    {
        _classRepoMock
            .Setup(r => r.GetByIdWithEnrollmentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);
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
        var details = Session(1, capacity: 10);
        SetupClassExists(details);
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
            .Setup(r => r.GetByIdWithEnrollmentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GymClassDetails?)null);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 999);
        await act.Should().ThrowAsync<ClassNotFoundError>();
    }

    [Fact]
    public async Task CreateAsync_ClassFull_ThrowsClassFullError()
    {
        var details = Session(1, capacity: 1, enrollmentClients: 1001);
        SetupClassExists(details);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 1);
        await act.Should().ThrowAsync<ClassFullError>();
    }

    [Fact]
    public async Task CreateAsync_AlreadyEnrolled_ThrowsClientAlreadyEnrolledError()
    {
        var details = Session(1, capacity: 10);
        SetupClassExists(details);
        SetupClientAlreadyEnrolled();

        var act = () => _factory.CreateAsync(clientId: 1, classId: 1);
        await act.Should().ThrowAsync<ClientAlreadyEnrolledError>();
    }
}
