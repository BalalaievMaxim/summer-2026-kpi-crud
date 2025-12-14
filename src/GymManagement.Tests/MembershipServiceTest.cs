using Xunit;
using Moq;
using GymManagement.Application.Services;
using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Core.Enums;

namespace GymManagement.Tests.Services;

public class MembershipServicesTests
{
    private readonly Mock<IMembershipPlanRepository> _mockPlanRepo;
    private readonly Mock<IMembershipRepository> _mockMembershipRepo;
    private readonly Mock<IClientRepository> _mockClientRepo;
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepo;
    private readonly Mock<IInvoiceService> _mockInvoiceService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    public MembershipServicesTests()
    {
        _mockPlanRepo = new Mock<IMembershipPlanRepository>();
        _mockMembershipRepo = new Mock<IMembershipRepository>();
        _mockClientRepo = new Mock<IClientRepository>();
        _mockInvoiceRepo = new Mock<IInvoiceRepository>();
        _mockInvoiceService = new Mock<IInvoiceService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task PurchaseMembershipAsync_ShouldSuccess_WhenDataValidAndNoActiveMembership()
    {
        var service = new MembershipService(
            _mockMembershipRepo.Object,
            _mockClientRepo.Object,
            _mockInvoiceService.Object,
            _mockPlanRepo.Object,
            _mockInvoiceRepo.Object,
            _mockUnitOfWork.Object
        );

        int clientId = 1;
        int planId = 5;

        _mockPlanRepo.Setup(r => r.GetMembershipPlanByIdAsync(planId))
            .ReturnsAsync(new MembershipPlan { PlanId = planId, DurationMonths = 1, Price = 100 });

        _mockClientRepo.Setup(r => r.GetClientByIdAsync(clientId))
            .ReturnsAsync(new Client { ClientId = clientId });

        _mockMembershipRepo.Setup(r => r.GetActiveMembershipsByClientAsync(clientId))
            .ReturnsAsync(new List<Membership>());

        var expectedInvoice = new Invoice { InvoiceId = 777, Amount = 100 };
        _mockInvoiceService.Setup(s => s.CreateInvoiceAsync(clientId, PaymentMethod.Card, planId, It.IsAny<string>()))
            .ReturnsAsync(expectedInvoice);

        await service.PurchaseMembershipAsync(clientId, planId, PaymentMethod.Card, "Note");

        _mockInvoiceRepo.Verify(r => r.AddAsync(expectedInvoice), Times.Once);

        _mockMembershipRepo.Verify(r => r.AddAsync(It.Is<Membership>(m => 
            m.ClientId == clientId && 
            m.PlanId == planId && 
            m.IsActive == false)), Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PurchaseMembershipAsync_ShouldThrowOrDoNothing_WhenAlreadyActive()
    {
        var service = new MembershipService(
            _mockMembershipRepo.Object,
            _mockClientRepo.Object,
            _mockInvoiceService.Object,
            _mockPlanRepo.Object,
            _mockInvoiceRepo.Object,
            _mockUnitOfWork.Object
        );

        int clientId = 1;
        int planId = 5;

        _mockPlanRepo.Setup(r => r.GetMembershipPlanByIdAsync(planId)).ReturnsAsync(new MembershipPlan { PlanId = planId });
        _mockClientRepo.Setup(r => r.GetClientByIdAsync(clientId)).ReturnsAsync(new Client { ClientId = clientId });

        _mockMembershipRepo.Setup(r => r.GetActiveMembershipsByClientAsync(clientId))
            .ReturnsAsync(new List<Membership> 
            { 
                new Membership { PlanId = planId, IsActive = true } 
            });

        await Assert.ThrowsAsync<Exception>(() => 
             service.PurchaseMembershipAsync(clientId, planId, PaymentMethod.Card, null));

        _mockMembershipRepo.Verify(r => r.AddAsync(It.IsAny<Membership>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PurchaseMembershipAsync_ShouldFail_WhenPlanOrClientNotFound()
    {
        var service = new MembershipService(
            _mockMembershipRepo.Object,
            _mockClientRepo.Object,
            _mockInvoiceService.Object,
            _mockPlanRepo.Object,
            _mockInvoiceRepo.Object,
            _mockUnitOfWork.Object
        );

        _mockClientRepo.Setup(r => r.GetClientByIdAsync(It.IsAny<int>())).ReturnsAsync(new Client());
        _mockPlanRepo.Setup(r => r.GetMembershipPlanByIdAsync(It.IsAny<int>())).ReturnsAsync((MembershipPlan?)null);

        await Assert.ThrowsAsync<Exception>(() => 
            service.PurchaseMembershipAsync(1, 999, PaymentMethod.Card, null));

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}