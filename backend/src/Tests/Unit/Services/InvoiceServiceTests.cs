using FluentAssertions;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Services;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Billing.Errors;
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
    private readonly InvoiceFactory _invoiceFactory;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _planRepoMock = new Mock<IMembershipPlanRepositoryPort>();
        _clientRepoMock = new Mock<IClientRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepositoryPort>();
        _membershipRepoMock = new Mock<IMembershipRepositoryPort>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _invoiceFactory = new InvoiceFactory(
            _clientRepoMock.Object,
            _planRepoMock.Object);

        _service = new InvoiceService(
            _invoiceRepoMock.Object,
            _membershipRepoMock.Object,
            _invoiceFactory,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdatePaidInvoice_StatusPending_Should_ChangeToPaid_And_ActivateMembership()
    {
        var invoiceId = 1;
        var clientId = 99;
        var aggregate = Invoice.Reconstitute(
            id: invoiceId,
            clientId: clientId,
            amount: 100m,
            date: DateOnly.FromDateTime(DateTime.UtcNow),
            status: PaymentStatus.Pending,
            method: PaymentMethod.Cash,
            notes: null);
        var pendingMembership = new MembershipRecord(5, clientId, 1, DateOnly.MinValue, DateOnly.MaxValue, false);

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);
        _membershipRepoMock.Setup(r => r.GetPendingMembershipByClientAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(pendingMembership);

        await _service.UpdatePaidInvoiceAsync(invoiceId);

        aggregate.Status.Should().Be(PaymentStatus.Paid);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Invoice>(inv => inv.Id == invoiceId && inv.Status == PaymentStatus.Paid),
            It.IsAny<CancellationToken>()), Times.Once);
        _membershipRepoMock.Verify(r => r.MarkAsActiveMembershipAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePaidInvoice_AlreadyPaid_Should_DoNothing()
    {
        var invoiceId = 1;
        var aggregate = Invoice.Reconstitute(
            id: invoiceId,
            clientId: 1,
            amount: 50m,
            date: DateOnly.FromDateTime(DateTime.UtcNow),
            status: PaymentStatus.Paid,
            method: PaymentMethod.Cash,
            notes: null);

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);

        await _service.UpdatePaidInvoiceAsync(invoiceId);

        _invoiceRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
        _membershipRepoMock.Verify(r => r.MarkAsActiveMembershipAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePaidInvoice_NotFound_Should_ThrowNotFoundException()
    {
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);

        var act = async () => await _service.UpdatePaidInvoiceAsync(42);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateInvoice_ValidData_Should_DelegateToFactoryAndPersist()
    {
        var clientId = 1;
        var planId = 1;
        var plan = new MembershipPlanSnapshot(1, "Basic", 3, 500m);

        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(planId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _clientRepoMock.Setup(r => r.ExistsAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await _service.CreateInvoiceAsync(clientId, PaymentMethod.Cash, planId, "Test");

        result.Should().NotBeNull();
        result.InvoiceId.Should().Be(42);
        result.ClientId.Should().Be(clientId);
        result.Amount.Should().Be(500);
        result.Status.Should().Be("pending");
        result.PaymentMethod.Should().Be("cash");
        result.Notes.Should().Be("Test");

        _invoiceRepoMock.Verify(r => r.AddAsync(
            It.Is<Invoice>(i =>
                i.ClientId == clientId &&
                i.Amount == 500m &&
                i.Status == PaymentStatus.Pending &&
                i.Method == PaymentMethod.Cash),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ClientNotFound_Should_ThrowDomainError()
    {
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MembershipPlanSnapshot(1, "P", 1, 10m));
        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await _service.CreateInvoiceAsync(1, PaymentMethod.Card, 1, null);

        await act.Should().ThrowAsync<ClientNotFoundForInvoiceError>();
    }

    [Fact]
    public async Task CreateInvoice_PlanNotFound_Should_ThrowDomainError()
    {
        _planRepoMock.Setup(r => r.GetMembershipPlanByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MembershipPlanSnapshot?)null);

        var act = async () => await _service.CreateInvoiceAsync(1, PaymentMethod.Card, 1, null);

        await act.Should().ThrowAsync<MembershipPlanNotFoundForInvoiceError>();
    }
}
