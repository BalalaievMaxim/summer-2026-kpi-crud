using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;
using Moq;

namespace GymManagement.Tests.Unit.Services;

public sealed class MembershipPlanServiceTests
{
    private readonly Mock<IMembershipPlanRepositoryPort> _planRepoMock;
    private readonly Mock<IMembershipRepositoryPort> _membershipRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MembershipPlanService _service;

    public MembershipPlanServiceTests()
    {
        _planRepoMock = new Mock<IMembershipPlanRepositoryPort>();
        _membershipRepoMock = new Mock<IMembershipRepositoryPort>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new MembershipPlanService(_planRepoMock.Object, _membershipRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Theory]
    [InlineData("", 1, 100)]
    [InlineData("Plan", 0, 100)]
    [InlineData("Plan", 1, -10)]
    public async Task CreatePlan_InvalidData_Should_ThrowDomainError(string name, int duration, decimal price)
    {
        var dto = new CreateMembershipPlanDto { Name = name, DurationMonth = duration, Price = price };

        var act = async () => await _service.CreatePlanAsync(dto);

        await act.Should().ThrowAsync<Exception>();
        _planRepoMock.Verify(r => r.AddAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUnusedPlan_WithActiveMemberships_Should_ThrowException()
    {
        var planId = 1;
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(planId, "Gold", 12, 500m));

        _membershipRepoMock.Setup(r => r.HasActiveMembershipsForPlanAsync(
                planId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _service.DeleteUnusedPlanAsync(planId);

        await act.Should().ThrowAsync<MembershipPlanInUseError>();

        _planRepoMock.Verify(r => r.DeleteMembershipPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePlan_ValidData_Should_AddAndSave()
    {
        var dto = new CreateMembershipPlanDto { Name = "Gold", DurationMonth = 12, Price = 1000 };

        await _service.CreatePlanAsync(dto);

        _planRepoMock.Verify(r => r.AddAsync(It.Is<MembershipPlan>(p =>
            p.Name == "Gold" &&
            p.DurationMonths == 12 &&
            p.Price.Amount == 1000), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUnusedPlan_NoActiveMemberships_Should_DeleteAndSave()
    {
        var planId = 1;
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(planId, "Gold", 12, 500m));
        _membershipRepoMock.Setup(r => r.HasActiveMembershipsForPlanAsync(
                planId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _service.DeleteUnusedPlanAsync(planId);

        _planRepoMock.Verify(r => r.DeleteMembershipPlanAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
