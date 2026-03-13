using FluentAssertions;
using GymManagement.Application.Services;
using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Exceptions;
using GymManagement.Core.Interfaces;
using Moq;
using Xunit;

namespace GymManagement.UnitTests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IMembershipPlanRepository> _planRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IMembershipRepository> _membershipRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _planRepoMock = new Mock<IMembershipPlanRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _membershipRepoMock = new Mock<IMembershipRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new InvoiceService(
            _planRepoMock.Object, _clientRepoMock.Object, _invoiceRepoMock.Object,
            _membershipRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdatePaidInvoice_StatusPending_Should_ChangeToPaid_And_ActivateMembership()
    {
        var invoiceId = 1;
        var clientId = 99;
        var invoice = new Invoice { InvoiceId = invoiceId, ClientId = clientId, Status = "pending" };
        var pendingMembership = new Membership { MembershipId = 5, ClientId = clientId };

        _invoiceRepoMock.Setup(r => r.GetInvoiceAsync(invoiceId)).ReturnsAsync(invoice);
        _membershipRepoMock.Setup(r => r.GetPendingMembershipByClientAsync(clientId)).ReturnsAsync(pendingMembership);

        await _service.UpdatePaidInvoiceAsync(invoiceId);

        invoice.Status.Should().Be("paid");

        _membershipRepoMock.Verify(r => r.MarkAsActiveMembershipAsync(5), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdatePaidInvoice_AlreadyPaid_Should_DoNothing()
    {
        var invoiceId = 1;
        var invoice = new Invoice { InvoiceId = invoiceId, Status = "paid" };

        _invoiceRepoMock.Setup(r => r.GetInvoiceAsync(invoiceId)).ReturnsAsync(invoice);

        await _service.UpdatePaidInvoiceAsync(invoiceId);

        _membershipRepoMock.Verify(r => r.MarkAsActiveMembershipAsync(It.IsAny<int>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateInvoice_ValidData_Should_CreateAndSave()
    {
        var clientId = 1;
        var planId = 1;
        var plan = new MembershipPlan { Price = 500 };
        var client = new Client { ClientId = clientId };

        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId)).ReturnsAsync(plan);
        _clientRepoMock.Setup(r => r.GetClientByIdAsync(clientId)).ReturnsAsync(client);

        var result = await _service.CreateInvoiceAsync(clientId, PaymentMethod.Cash, planId, "Test");

        result.Should().NotBeNull();
        result.Amount.Should().Be(500);
        result.Status.Should().Be("pending");
        _invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ClientNotFound_Should_ThrowNotFoundException()
    {
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(1)).ReturnsAsync(new MembershipPlan());
        _clientRepoMock.Setup(r => r.GetClientByIdAsync(1)).ReturnsAsync((Client)null!);

        var act = async () => await _service.CreateInvoiceAsync(1, PaymentMethod.Card, 1, null);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Client with ID 1 not found.");
    }
}