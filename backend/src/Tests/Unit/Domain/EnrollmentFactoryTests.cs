using FluentAssertions;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;
using DomainClass = GymManagement.Domain.Classes.Class;

namespace GymManagement.Tests.Unit.Domain;

public sealed class EnrollmentFactoryTests
{
    private readonly Mock<IClassRepositoryPort> _classRepoMock = new();
    private readonly EnrollmentFactory _factory;
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    public EnrollmentFactoryTests()
    {
        _factory = new EnrollmentFactory(_classRepoMock.Object);
    }

    private static DomainClass Session(int classId, int capacity, params int[] enrollmentClients)
    {
        var schedule = TimeRange.Create(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(2));
        var enrollments = enrollmentClients.Select((clientId, index) =>
            Enrollment.Reconstitute(index + 1, clientId, classId, DateTimeOffset.UtcNow));

        return DomainClass.Reconstitute(classId, 1, 1, schedule, capacity, enrollments);
    }

    private void SetupClassExists(DomainClass details)
    {
        _classRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsEnrollment()
    {
        var details = Session(1, capacity: 10);
        SetupClassExists(details);

        var enrollment = await _factory.CreateAsync(clientId: 1, classId: 1, Now);

        enrollment.Should().NotBeNull();
        enrollment.ClientId.Should().Be(1);
        enrollment.ClassId.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_InvalidClientId_ThrowsInvalidEnrollmentError()
    {
        var act = () => _factory.CreateAsync(clientId: 0, classId: 1, Now);
        await act.Should().ThrowAsync<InvalidEnrollmentError>();
    }

    [Fact]
    public async Task CreateAsync_InvalidClassId_ThrowsInvalidEnrollmentError()
    {
        var act = () => _factory.CreateAsync(clientId: 1, classId: -1, Now);
        await act.Should().ThrowAsync<InvalidEnrollmentError>();
    }

    [Fact]
    public async Task CreateAsync_ClassNotFound_ThrowsClassNotFoundError()
    {
        _classRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainClass?)null);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 999, Now);
        await act.Should().ThrowAsync<ClassNotFoundError>();
    }

    [Fact]
    public async Task CreateAsync_ClassFull_ThrowsClassFullError()
    {
        var details = Session(1, capacity: 1, enrollmentClients: 1001);
        SetupClassExists(details);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 1, Now);
        await act.Should().ThrowAsync<ClassFullError>();
    }

    [Fact]
    public async Task CreateAsync_AlreadyEnrolled_ThrowsDuplicateEnrollmentError()
    {
        var details = Session(1, capacity: 10, enrollmentClients: 1);
        SetupClassExists(details);

        var act = () => _factory.CreateAsync(clientId: 1, classId: 1, Now);
        await act.Should().ThrowAsync<DuplicateEnrollmentError>();
    }
}
