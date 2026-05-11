using FluentAssertions;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Services;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;
using Moq;

namespace GymManagement.Tests.Unit.Services;

public sealed class InvoiceServiceTests
{
    private readonly Mock<IMembershipPlanRepositoryPort> _planRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IInvoiceRepositoryPort> _invoiceRepoMock;
    private readonly Mock<IMembershipRepositoryPort> _membershipRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _planRepoMock = new Mock<IMembershipPlanRepositoryPort>();
        _clientRepoMock = new Mock<IClientRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepositoryPort>();
        _membershipRepoMock = new Mock<IMembershipRepositoryPort>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new InvoiceService(
            _planRepoMock.Object,
            _clientRepoMock.Object,
            _invoiceRepoMock.Object,
            _membershipRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdatePaidInvoice_StatusPending_Should_ChangeToPaid_And_ActivateMembership()
    {
        var invoiceId = 1;
        var clientId = 99;
        var invoice = new InvoiceRecord(invoiceId, clientId, 100m, DateOnly.FromDateTime(DateTime.UtcNow), "pending", "cash", null);
        var pendingMembership = new MembershipRecord(5, clientId, 1, DateOnly.MinValue, DateOnly.MaxValue, false);

        _invoiceRepoMock.Setup(r => r.GetInvoiceAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _membershipRepoMock.Setup(r => r.GetPendingMembershipByClientAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(pendingMembership);

        await _service.UpdatePaidInvoiceAsync(invoiceId);

        _invoiceRepoMock.Verify(r => r.UpdateStatusAsync(invoiceId, "paid", It.IsAny<CancellationToken>()), Times.Once);
        _membershipRepoMock.Verify(r => r.MarkAsActiveMembershipAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePaidInvoice_AlreadyPaid_Should_DoNothing()
    {
        var invoiceId = 1;
        var invoice = new InvoiceRecord(invoiceId, 1, 50m, DateOnly.FromDateTime(DateTime.UtcNow), "paid", "cash", null);

        _invoiceRepoMock.Setup(r => r.GetInvoiceAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        await _service.UpdatePaidInvoiceAsync(invoiceId);

        _invoiceRepoMock.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _membershipRepoMock.Verify(r => r.MarkAsActiveMembershipAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateInvoice_ValidData_Should_CreateAndSave()
    {
        var clientId = 1;
        var planId = 1;
        var plan = new MembershipPlanSnapshot(1, "Basic", 3, 500m);

        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _clientRepoMock.Setup(r => r.ExistsAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var created = new InvoiceRecord(42, clientId, 500m, DateOnly.FromDateTime(DateTime.UtcNow), "pending", "cash", "Test");
        _invoiceRepoMock.Setup(r => r.GetPendingInvoicesAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<InvoiceRecord> { created });

        var result = await _service.CreateInvoiceAsync(clientId, PaymentMethod.Cash, planId, "Test");

        result.Should().NotBeNull();
        result.Amount.Should().Be(500);
        result.Status.Should().Be("pending");
        _invoiceRepoMock.Verify(r => r.AddAsync(It.Is<InvoiceRecord>(i => i.ClientId == clientId && i.Amount == 500m), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ClientNotFound_Should_ThrowNotFoundException()
    {
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new MembershipPlanSnapshot(1, "P", 1, 10m));
        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _service.CreateInvoiceAsync(1, PaymentMethod.Card, 1, null);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Client with ID 1 not found.");
    }
}
