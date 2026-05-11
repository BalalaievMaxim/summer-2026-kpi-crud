using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Domain.Memberships;
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
    public async Task CreatePlan_InvalidData_Should_ThrowArgumentException(string name, int duration, decimal price)
    {
        var dto = new CreateMembershipPlanDto { Name = name, DurationMonth = duration, Price = price };

        var act = async () => await _service.CreatePlanAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
        _planRepoMock.Verify(r => r.AddAsync(It.IsAny<MembershipPlanSnapshot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUnusedPlan_WithActiveMemberships_Should_ThrowException()
    {
        var planId = 1;
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId, It.IsAny<CancellationToken>())).ReturnsAsync(new MembershipPlanSnapshot(planId, "Gold", 12, 500m));

        _membershipRepoMock.Setup(r => r.GetAllActiveMembershipReferencedOnMembershipPlan(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MembershipRecord> { new(1, 1, planId, DateOnly.MinValue, DateOnly.MaxValue, true) });

        var act = async () => await _service.DeleteUnusedPlanAsync(planId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete plan. There are active memberships associated with it.");

        _planRepoMock.Verify(r => r.DeleteMembershipPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePlan_ValidData_Should_AddAndSave()
    {
        var dto = new CreateMembershipPlanDto { Name = "Gold", DurationMonth = 12, Price = 1000 };

        await _service.CreatePlanAsync(dto);

        _planRepoMock.Verify(r => r.AddAsync(It.Is<MembershipPlanSnapshot>(p =>
            p.Name == "Gold" &&
            p.DurationMonths == 12 &&
            p.Price == 1000), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUnusedPlan_NoActiveMemberships_Should_DeleteAndSave()
    {
        var planId = 1;
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId, It.IsAny<CancellationToken>())).ReturnsAsync(new MembershipPlanSnapshot(planId, "Gold", 12, 500m));
        _membershipRepoMock.Setup(r => r.GetAllActiveMembershipReferencedOnMembershipPlan(planId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<MembershipRecord>());

        await _service.DeleteUnusedPlanAsync(planId);

        _planRepoMock.Verify(r => r.DeleteMembershipPlanAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
