using Moq;
using GymManagement.Application.Services;
using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;

namespace GymManagement.Tests;

public class MembershipPlanServiceTest
{
    
    private readonly Mock<IMembershipPlanRepository> _mockPlanRepo;
    private readonly Mock<IMembershipRepository> _mockMembershipRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public MembershipPlanServiceTest()
    {
        _mockPlanRepo = new Mock<IMembershipPlanRepository>();
        _mockMembershipRepo = new Mock<IMembershipRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }
    
    
    [Fact]
    public async Task CreatePlanAsync_ShouldAddPlan_WhenDtoIsValid()
    {
        var service = new MembershipPlanService(
            _mockPlanRepo.Object, 
            _mockMembershipRepo.Object, 
            _mockUnitOfWork.Object);

        var dto = new CreateMembershipPlanDto 
        { 
            Name = "Premium Plan", 
            DurationMonth = 6, 
            Price = 500 
        };
        
        await service.CreatePlanAsync(dto);

        _mockPlanRepo.Verify(x => x.AddAsync(It.Is<MembershipPlan>(p => 
            p.Name == dto.Name && 
            p.Price == dto.Price &&
            p.DurationMonths == dto.DurationMonth)), Times.Once);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldNotAddPlan_WhenDtoIsInvalid()
    {
        var service = new MembershipPlanService(
            _mockPlanRepo.Object, 
            _mockMembershipRepo.Object, 
            _mockUnitOfWork.Object);

        var invalidDto = new CreateMembershipPlanDto 
        { 
            Name = "", 
            DurationMonth = 0, 
            Price = 0 
        };

        await service.CreatePlanAsync(invalidDto);
        _mockPlanRepo.Verify(x => x.AddAsync(It.IsAny<MembershipPlan>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUnusedPlanAsync_ShouldDelete_WhenNoActiveMemberships()
    {
        var service = new MembershipPlanService(
            _mockPlanRepo.Object, 
            _mockMembershipRepo.Object, 
            _mockUnitOfWork.Object);
        
        int planId = 100;
        
        _mockPlanRepo.Setup(r => r.GetMembershipPlanByIdAsync(planId))
            .ReturnsAsync(new MembershipPlan { PlanId = planId });
        
        _mockMembershipRepo.Setup(r => r.GetAllActiveMembershipReferencedOnMembershipPlan(planId))
            .ReturnsAsync(new List<Membership>());
        
        await service.DeleteUnusedPlanAsync(planId);

        _mockPlanRepo.Verify(r => r.DeleteMembershipPlanAsync(planId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUnusedPlanAsync_ShouldThrowException_WhenHasActiveMemberships()
    {
        var service = new MembershipPlanService(
            _mockPlanRepo.Object, 
            _mockMembershipRepo.Object, 
            _mockUnitOfWork.Object);
        
        int planId = 100;

        _mockPlanRepo.Setup(r => r.GetMembershipPlanByIdAsync(planId))
            .ReturnsAsync(new MembershipPlan { PlanId = planId });

        _mockMembershipRepo.Setup(r => r.GetAllActiveMembershipReferencedOnMembershipPlan(planId))
            .ReturnsAsync(new List<Membership> { new Membership() });

        var exception = await Assert.ThrowsAsync<Exception>(() => service.DeleteUnusedPlanAsync(planId));
        Assert.Equal("The plan has active memberships", exception.Message);

        _mockPlanRepo.Verify(r => r.DeleteMembershipPlanAsync(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}