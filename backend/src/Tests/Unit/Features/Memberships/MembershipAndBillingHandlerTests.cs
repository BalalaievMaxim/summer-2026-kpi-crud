using FluentAssertions;
using GymManagement.Application.Features.Enrollments.Commands.CreateEnrollment;
using GymManagement.Application.Features.Invoices.Commands.CreateInvoice;
using GymManagement.Application.Features.Invoices.Commands.MarkInvoicePaid;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;
using GymManagement.Application.Features.Memberships.Commands.PurchaseMembership;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Billing.Errors;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;
using DomainClass = GymManagement.Domain.Classes.Class;

namespace GymManagement.Tests.Unit.Features.Memberships;

public sealed class MembershipAndBillingHandlerTests
{
    private readonly Mock<IMembershipRepositoryPort> _membershipRepoMock = new();
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<IInvoiceRepositoryPort> _invoiceRepoMock = new();
    private readonly Mock<IMembershipPlanRepositoryPort> _planRepoMock = new();
    private readonly Mock<IEnrollmentRepositoryPort> _enrollmentRepoMock = new();
    private readonly Mock<IClassRepositoryPort> _classRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly InvoiceFactory _invoiceFactory;
    private readonly EnrollmentFactory _enrollmentFactory;

    public MembershipAndBillingHandlerTests()
    {
        _invoiceFactory = new InvoiceFactory(_clientRepoMock.Object, _planRepoMock.Object);
        _enrollmentFactory = new EnrollmentFactory(_classRepoMock.Object);
    }

    private static DomainClass Session(int classId, int capacity, params int[] enrollmentClients)
    {
        var schedule = TimeRange.Create(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1));
        var enrollments = enrollmentClients.Select((clientId, index) =>
            Enrollment.Reconstitute(index + 1, clientId, classId, DateTimeOffset.UtcNow));

        return DomainClass.Reconstitute(classId, 1, 1, schedule, capacity, enrollments);
    }

    private void SetupActiveMembership(int clientId)
        => _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Membership>
            {
                Membership.Reconstitute(
                    1,
                    clientId,
                    1,
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    true)
            });

    [Fact]
    public async Task PurchaseMembership_AlreadyHasActive_Should_ThrowDomainError()
    {
        var handler = new PurchaseMembershipCommandHandler(
            _membershipRepoMock.Object,
            _clientRepoMock.Object,
            _invoiceRepoMock.Object,
            _planRepoMock.Object,
            _invoiceFactory,
            _unitOfWorkMock.Object);

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(1, "P", 1, 100m));
        _membershipRepoMock.Setup(r => r.HasActiveMembershipForPlanAsync(1, 1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await handler.Handle(new PurchaseMembershipCommand(1, 1, PaymentMethod.Card, "Notes"));

        await act.Should().ThrowAsync<ActiveMembershipExistsError>();
        _membershipRepoMock.Verify(r => r.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()), Times.Never);
        _invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PurchaseMembership_ValidData_Should_CreateInvoice_AddMembership_AndSave()
    {
        var handler = new PurchaseMembershipCommandHandler(
            _membershipRepoMock.Object,
            _clientRepoMock.Object,
            _invoiceRepoMock.Object,
            _planRepoMock.Object,
            _invoiceFactory,
            _unitOfWorkMock.Object);

        var plan = MembershipPlan.Reconstitute(1, "SixMo", 6, 200m);

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _membershipRepoMock.Setup(r => r.HasActiveMembershipForPlanAsync(1, 1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        await handler.Handle(new PurchaseMembershipCommand(1, 1, PaymentMethod.Card, "Notes"));

        _invoiceRepoMock.Verify(r => r.Stage(It.Is<Invoice>(invoice =>
            invoice.ClientId == 1 &&
            invoice.Amount == 200m &&
            invoice.Status == PaymentStatus.Pending &&
            invoice.Method == PaymentMethod.Card)), Times.Once);
        _membershipRepoMock.Verify(r => r.AddAsync(It.Is<Membership>(membership =>
            membership.ClientId == 1 &&
            membership.PlanId == 1 &&
            membership.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", 1, 100)]
    [InlineData("Plan", 0, 100)]
    [InlineData("Plan", 1, -10)]
    public async Task CreateMembershipPlan_InvalidData_Should_ThrowDomainError(string name, int duration, decimal price)
    {
        var handler = new CreateMembershipPlanCommandHandler(_planRepoMock.Object, _unitOfWorkMock.Object);

        var act = async () => await handler.Handle(new CreateMembershipPlanCommand(name, duration, price));

        await act.Should().ThrowAsync<Exception>();
        _planRepoMock.Verify(r => r.AddAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembershipPlan_ValidData_Should_AddAndSave()
    {
        var handler = new CreateMembershipPlanCommandHandler(_planRepoMock.Object, _unitOfWorkMock.Object);

        await handler.Handle(new CreateMembershipPlanCommand("Gold", 12, 1000));

        _planRepoMock.Verify(r => r.AddAsync(It.Is<MembershipPlan>(plan =>
            plan.Name == "Gold" &&
            plan.DurationMonths == 12 &&
            plan.Price.Amount == 1000), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMembershipPlan_WithActiveMemberships_Should_ThrowDomainError()
    {
        var handler = new DeleteMembershipPlanCommandHandler(
            _planRepoMock.Object,
            _membershipRepoMock.Object,
            _unitOfWorkMock.Object);

        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(1, "Gold", 12, 500m));
        _membershipRepoMock.Setup(r => r.HasActiveMembershipsForPlanAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await handler.Handle(new DeleteMembershipPlanCommand(1));

        await act.Should().ThrowAsync<MembershipPlanInUseError>();
        _planRepoMock.Verify(r => r.DeleteMembershipPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMembershipPlan_NoActiveMemberships_Should_DeleteAndSave()
    {
        var handler = new DeleteMembershipPlanCommandHandler(
            _planRepoMock.Object,
            _membershipRepoMock.Object,
            _unitOfWorkMock.Object);

        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(1, "Gold", 12, 500m));
        _membershipRepoMock.Setup(r => r.HasActiveMembershipsForPlanAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await handler.Handle(new DeleteMembershipPlanCommand(1));

        _planRepoMock.Verify(r => r.DeleteMembershipPlanAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ValidData_Should_DelegateToFactoryAndPersist()
    {
        var handler = new CreateInvoiceCommandHandler(_invoiceRepoMock.Object, _invoiceFactory);

        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(1, "Basic", 3, 500m));
        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await handler.Handle(new CreateInvoiceCommand(1, PaymentMethod.Cash, 1, "Test"));

        result.Should().Be(42);
        _invoiceRepoMock.Verify(r => r.AddAsync(It.Is<Invoice>(invoice =>
            invoice.ClientId == 1 &&
            invoice.Amount == 500m &&
            invoice.Status == PaymentStatus.Pending &&
            invoice.Method == PaymentMethod.Cash), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ClientNotFound_Should_ThrowDomainError()
    {
        var handler = new CreateInvoiceCommandHandler(_invoiceRepoMock.Object, _invoiceFactory);

        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(1, "P", 1, 10m));
        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await handler.Handle(new CreateInvoiceCommand(1, PaymentMethod.Card, 1, null));

        await act.Should().ThrowAsync<ClientNotFoundForInvoiceError>();
    }

    [Fact]
    public async Task MarkInvoicePaid_StatusPending_Should_ChangeToPaid_And_ActivateMembership()
    {
        var handler = new MarkInvoicePaidCommandHandler(
            _invoiceRepoMock.Object,
            _membershipRepoMock.Object,
            _unitOfWorkMock.Object);

        var invoice = Invoice.Reconstitute(1, 99, 100m, DateOnly.FromDateTime(DateTime.UtcNow), PaymentStatus.Pending, PaymentMethod.Cash, null);
        var pendingMembership = Membership.Reconstitute(
            5,
            99,
            1,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            false);

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _membershipRepoMock.Setup(r => r.GetPendingMembershipByClientAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(pendingMembership);

        await handler.Handle(new MarkInvoicePaidCommand(1));

        _invoiceRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Invoice>(updated => updated.Id == 1 && updated.Status == PaymentStatus.Paid),
            It.IsAny<CancellationToken>()), Times.Once);
        _membershipRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Membership>(membership => membership.Id == 5 && membership.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEnrollment_ValidData_Should_CreateAndReturnResult()
    {
        var handler = new CreateEnrollmentCommandHandler(
            _enrollmentRepoMock.Object,
            _clientRepoMock.Object,
            _membershipRepoMock.Object,
            _enrollmentFactory);

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 10));
        _enrollmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await handler.Handle(new CreateEnrollmentCommand(1, 1));

        result.EnrollmentId.Should().Be(42);
        result.ClientId.Should().Be(1);
        result.ClassId.Should().Be(1);
        _enrollmentRepoMock.Verify(r => r.AddAsync(It.Is<Enrollment>(enrollment =>
            enrollment.ClientId == 1 &&
            enrollment.ClassId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEnrollment_ClassIsFull_Should_ThrowDomainError()
    {
        var handler = new CreateEnrollmentCommandHandler(
            _enrollmentRepoMock.Object,
            _clientRepoMock.Object,
            _membershipRepoMock.Object,
            _enrollmentFactory);

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupActiveMembership(1);
        _classRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Session(1, 1, 2));

        var act = async () => await handler.Handle(new CreateEnrollmentCommand(1, 1));

        await act.Should().ThrowAsync<ClassFullError>();
        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEnrollment_NoActiveMembership_Should_ThrowDomainError()
    {
        var handler = new CreateEnrollmentCommandHandler(
            _enrollmentRepoMock.Object,
            _clientRepoMock.Object,
            _membershipRepoMock.Object,
            _enrollmentFactory);

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _membershipRepoMock.Setup(r => r.GetActiveMembershipsByClientAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Membership>());

        var act = async () => await handler.Handle(new CreateEnrollmentCommand(1, 1));

        await act.Should().ThrowAsync<ClientHasNoActiveMembershipError>();
    }
}
