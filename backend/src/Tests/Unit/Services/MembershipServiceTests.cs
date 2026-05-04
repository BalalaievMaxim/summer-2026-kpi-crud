using FluentAssertions;
using GymManagement.Application.Services;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Entities.Enums;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Application.Services.Interfaces;
using Moq;
using Xunit;

namespace GymManagement.Tests.Unit.Services;

public class MembershipServiceTests
{
    private readonly Mock<IMembershipRepository> _membershipRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly Mock<IMembershipPlanRepository> _planRepoMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MembershipService _service;

    public MembershipServiceTests()
    {
        _membershipRepoMock = new Mock<IMembershipRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _planRepoMock = new Mock<IMembershipPlanRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new MembershipService(
            _membershipRepoMock.Object, _clientRepoMock.Object, _invoiceServiceMock.Object,
            _planRepoMock.Object, _invoiceRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task PurchaseMembership_AlreadyHasActive_Should_ThrowException()
    {
        var clientId = 1;
        var planId = 1;

        _clientRepoMock.Setup(r => r.GetClientByIdAsync(clientId)).ReturnsAsync(new Client());
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId)).ReturnsAsync(new MembershipPlan());
        
        var activeMemberships = new List<Membership> { new Membership { PlanId = planId, IsActive = true } };
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(clientId)).ReturnsAsync(activeMemberships);

        var act = async () => await _service.PurchaseMembershipAsync(clientId, planId, PaymentMethod.Card, "Notes");

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Client already has an active membership for this plan.");
                 
        _membershipRepoMock.Verify(r => r.AddAsync(It.IsAny<Membership>()), Times.Never);
    }
    [Fact]
    public async Task PurchaseMembership_ValidData_Should_AddMembership_And_CreateInvoice()
    {
        var clientId = 1;
        var planId = 1;
        var plan = new MembershipPlan { DurationMonths = 6 };

        _clientRepoMock.Setup(r => r.GetClientByIdAsync(clientId)).ReturnsAsync(new Client());
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId)).ReturnsAsync(plan);
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(clientId)).ReturnsAsync(new List<Membership>());

        await _service.PurchaseMembershipAsync(clientId, planId, PaymentMethod.Card, "Notes");

        _invoiceServiceMock.Verify(s => s.CreateInvoiceAsync(clientId, PaymentMethod.Card, planId, "Notes"), Times.Once);
        
        _membershipRepoMock.Verify(r => r.AddAsync(It.Is<Membership>(m => 
            m.ClientId == clientId && 
            m.PlanId == planId && 
            m.IsActive == false)), Times.Once);
            
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }
}