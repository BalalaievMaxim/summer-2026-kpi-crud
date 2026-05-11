using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;
using Moq;

namespace GymManagement.Tests.Unit.Services;

public sealed class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepositoryPort> _enrollmentRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IClassScheduleRepository> _classRepoMock;
    private readonly Mock<IMembershipRepositoryPort> _membershipRepoMock;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _enrollmentRepoMock = new Mock<IEnrollmentRepositoryPort>();
        _clientRepoMock = new Mock<IClientRepository>();
        _classRepoMock = new Mock<IClassScheduleRepository>();
        _membershipRepoMock = new Mock<IMembershipRepositoryPort>();

        var factory = new EnrollmentFactory(_classRepoMock.Object, _enrollmentRepoMock.Object);

        _service = new EnrollmentService(
            _enrollmentRepoMock.Object,
            _clientRepoMock.Object,
            _classRepoMock.Object,
            _membershipRepoMock.Object,
            factory);
    }

    private static GymClassDetails Session(int classId, int capacity, params int[] enrollmentClients)
        => new(classId, 1, "t", 1, "c", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), capacity, enrollmentClients);

    private void SetupActiveMembership(int clientId)
        => _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MembershipRecord>
            {
                new(1, clientId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), true)
            });

    [Fact]
    public async Task CreateEnrollment_ValidData_Should_CreateAndSave()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdWithEnrollmentsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 10));
        _enrollmentRepoMock.Setup(r => r.IsClientEnrolledAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _enrollmentRepoMock.Setup(r => r.AddAsync(1, 1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await _service.CreateEnrollmentAsync(dto);

        result.EnrollmentId.Should().Be(42);
        result.ClientId.Should().Be(1);
        result.ClassId.Should().Be(1);

        _enrollmentRepoMock.Verify(r => r.AddAsync(1, 1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEnrollment_ClassIsFull_Should_ThrowDomainError()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdWithEnrollmentsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 1, 2));

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<ClassFullError>();

        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEnrollment_NoActiveMembership_Should_ThrowException()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MembershipRecord>());

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client does not have an active membership.");
    }

    [Fact]
    public async Task CreateEnrollment_AlreadyEnrolled_Should_ThrowDomainError()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdWithEnrollmentsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 10));
        _enrollmentRepoMock.Setup(r => r.IsClientEnrolledAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<ClientAlreadyEnrolledError>();
    }
}
