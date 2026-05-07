using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Domain.Clients;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Application.Services.Interfaces;
using Moq;
using Xunit;

namespace GymManagement.Tests.Unit.Services;

public class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IClassRepository> _classRepoMock;
    private readonly Mock<IMembershipRepository> _membershipRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _enrollmentRepoMock = new Mock<IEnrollmentRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _classRepoMock = new Mock<IClassRepository>();
        _membershipRepoMock = new Mock<IMembershipRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new EnrollmentService(
            _enrollmentRepoMock.Object,
            _clientRepoMock.Object,
            _classRepoMock.Object,
            _membershipRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateEnrollment_ValidData_Should_CreateAndSave()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        var classEntity = new Class
        {
            ClassId = 1,
            Capacity = 10,
            Enrollments = new List<Enrollment>()
        };

        var activeMemberships = new List<Membership>
        {
            new Membership { IsActive = true, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) }
        };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _classRepoMock.Setup(repo => repo.GetByIdWithEnrollmentsAsync(1)).ReturnsAsync(classEntity);
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(1)).ReturnsAsync(activeMemberships);

        var result = await _service.CreateEnrollmentAsync(dto);

        result.Should().NotBeNull();
        result.ClientId.Should().Be(1);
        result.ClassId.Should().Be(1);

        _enrollmentRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Enrollment>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateEnrollment_ClassIsFull_Should_ThrowException()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        var classEntity = new Class
        {
            ClassId = 1,
            Capacity = 1,
            Enrollments = new List<Enrollment> { new Enrollment { ClientId = 2 } }
        };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _classRepoMock.Setup(repo => repo.GetByIdWithEnrollmentsAsync(1)).ReturnsAsync(classEntity);

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Class is full.");

        _enrollmentRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Enrollment>()), Times.Never);
    }

    [Fact]
    public async Task CreateEnrollment_NoActiveMembership_Should_ThrowException()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        var classEntity = new Class { ClassId = 1, Capacity = 10, Enrollments = new List<Enrollment>() };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _classRepoMock.Setup(repo => repo.GetByIdWithEnrollmentsAsync(1)).ReturnsAsync(classEntity);
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(1))
            .ReturnsAsync(new List<Membership> { new Membership { IsActive = false } });

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Client does not have an active membership.");
    }

    [Fact]
    public async Task CreateEnrollment_AlreadyEnrolled_Should_ThrowException()
    {
        var dto = new CreateEnrollmentDto { ClientId = 1, ClassId = 1 };

        var classEntity = new Class
        {
            ClassId = 1,
            Capacity = 10,
            Enrollments = new List<Enrollment> { new Enrollment { ClientId = 1 } }
        };

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _classRepoMock.Setup(r => r.GetByIdWithEnrollmentsAsync(1)).ReturnsAsync(classEntity);

        var act = async () => await _service.CreateEnrollmentAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client is already enrolled in this class.");
    }
}
