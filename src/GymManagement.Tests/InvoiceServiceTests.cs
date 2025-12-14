using Moq;
using GymManagement.Application.Services;
using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;

namespace GymManagement.Tests;


public class InvoiceServiceTests
{
    private readonly Mock<IMembershipPlanRepository> _mockPlanRepo;
    private readonly Mock<IClientRepository> _mockClientRepo;
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepo;
    private readonly Mock<IMembershipRepository> _mockMembershipRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    private readonly InvoiceService _service;


    public InvoiceServiceTests()
    {
        _mockPlanRepo = new Mock<IMembershipPlanRepository>();
        _mockClientRepo = new Mock<IClientRepository>();
        _mockInvoiceRepo = new Mock<IInvoiceRepository>();
        _mockMembershipRepo = new Mock<IMembershipRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _service = new InvoiceService(
            _mockPlanRepo.Object,
            _mockClientRepo.Object,
            _mockInvoiceRepo.Object,
            _mockMembershipRepo.Object,
            _mockUnitOfWork.Object
        );
    }
    
    [Fact]
    public async Task CreateInvoiceAsync_ShouldReturnInvoice_WhenClientAndPlanExist()
    {
        int clientId = 1;
        int planId = 10;
        decimal expectedPrice = 100.00m;

        _mockPlanRepo.Setup(repo => repo.GetMembershipPlanByIdAsync(planId))
            .ReturnsAsync(new MembershipPlan { PlanId = planId, Price = expectedPrice });

        _mockClientRepo.Setup(repo => repo.GetClientByIdAsync(clientId))
            .ReturnsAsync(new Client { ClientId = clientId, Name = "Test User" });

        var result = await _service.CreateInvoiceAsync(clientId, PaymentMethod.Card, planId, "Test Note");

        Assert.NotNull(result);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(expectedPrice, result.Amount);
        Assert.Equal("pending", result.Status);
        Assert.Equal("Test Note", result.Notes);
    }
    
    [Fact]
    public async Task CreateInvoiceAsync_ShouldReturnEmptyInvoice_WhenClientNotFound()
    {
        _mockClientRepo.Setup(repo => repo.GetClientByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Client?)null);
            
        _mockPlanRepo.Setup(repo => repo.GetMembershipPlanByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new MembershipPlan());

        var result = await _service.CreateInvoiceAsync(1, PaymentMethod.Cash, 1, null);

        Assert.Null(result.Client);
        Assert.Equal(0, result.Amount);
    }
    [Fact]
    public async Task UpdatePaidInvoiceAsync_ShouldMarkPaidAndSave_WhenInvoiceIsPending()
    {
        int clientId = 1;
        int invoiceId = 5;
        int firstMembershipId = 99;
        int secondMembershipId = 100;

        var pendingInvoice = new Invoice { InvoiceId = invoiceId, Status = "pending" };
        var clientMembership = new Membership { MembershipId = firstMembershipId };
        var client2Membership = new Membership { MembershipId = secondMembershipId };
        List<Membership> clientMemberships = new List<Membership>();
        clientMemberships.Add(client2Membership);
        clientMemberships.Add(clientMembership);

        _mockInvoiceRepo.Setup(r => r.GetInvoiceAsync(clientId, invoiceId))
            .ReturnsAsync(pendingInvoice);
        
        _mockMembershipRepo.Setup(r => r.GetActiveMembershipsByClientAsync(clientId))
            .ReturnsAsync(clientMemberships);

        await _service.UpdatePaidInvoiceAsync(clientId, invoiceId);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePaidInvoiceAsync_ShouldDoNothing_WhenInvoiceAlreadyPaid()
    {
        var paidInvoice = new Invoice { InvoiceId = 1, Status = "paid" };

        _mockInvoiceRepo.Setup(r => r.GetInvoiceAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paidInvoice);

        await _service.UpdatePaidInvoiceAsync(1, 1);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllPendingInvoicesAsync_ShouldFilterOnlyPending()
    {
        var allInvoices = new List<Invoice>
        {
            new Invoice { InvoiceId = 1, Status = "pending" },
            new Invoice { InvoiceId = 2, Status = "paid" },
            new Invoice { InvoiceId = 3, Status = "pending" },
            new Invoice { InvoiceId = 4, Status = "cancelled" }
        };

        _mockInvoiceRepo.Setup(r => r.GetAllClientInvoicesAsync(1))
            .ReturnsAsync(allInvoices);

        var result = await _service.GetAllPendingInvoicesAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, inv => Assert.Equal("pending", inv.Status));
    }
}
