using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;
using DomainClass = GymManagement.Domain.Classes.Class;

namespace GymManagement.Tests.Unit.Services;

public sealed class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepositoryPort> _enrollmentRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IClassRepositoryPort> _classRepoMock;
    private readonly Mock<IMembershipRepositoryPort> _membershipRepoMock;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _enrollmentRepoMock = new Mock<IEnrollmentRepositoryPort>();
        _clientRepoMock = new Mock<IClientRepository>();
        _classRepoMock = new Mock<IClassRepositoryPort>();
        _membershipRepoMock = new Mock<IMembershipRepositoryPort>();

        var factory = new EnrollmentFactory(_classRepoMock.Object);

        _service = new EnrollmentService(
            _enrollmentRepoMock.Object,
            _clientRepoMock.Object,
            _membershipRepoMock.Object,
            factory);
    }

    private static DomainClass Session(int classId, int capacity, params int[] enrollmentClients)
    {
        var schedule = TimeRange.Create(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1));
        var enrollments = enrollmentClients.Select((clientId, index) =>
            Enrollment.Reconstitute(index + 1, clientId, classId, DateTimeOffset.UtcNow));

        return DomainClass.Reconstitute(classId, 1, 1, schedule, capacity, enrollments);
    }

    private void SetupActiveMembership(int clientId)
        => _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Membership>
            {
                Membership.Reconstitute(
                    1,
                    clientId,
                    1,
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    true)
            });

    [Fact]
    public async Task CreateEnrollment_ValidData_Should_CreateAndSave()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 10));
        _enrollmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await _service.CreateEnrollmentAsync(dto);

        result.EnrollmentId.Should().Be(42);
        result.ClientId.Should().Be(1);
        result.ClassId.Should().Be(1);

        _enrollmentRepoMock.Verify(r => r.AddAsync(It.Is<Enrollment>(e =>
            e.ClientId == 1 &&
            e.ClassId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEnrollment_ClassIsFull_Should_ThrowDomainError()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 1, 2));

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<ClassFullError>();

        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEnrollment_NoActiveMembership_Should_ThrowException()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Membership>());

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<ClientHasNoActiveMembershipError>();
    }

    [Fact]
    public async Task CreateEnrollment_AlreadyEnrolled_Should_ThrowDomainError()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 10, 1));

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<DuplicateEnrollmentError>();
    }
}
