using FluentAssertions;
using GymManagement.Application.Services;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using Moq;

namespace GymManagement.Tests.Unit.Services;

public sealed class MembershipServiceTests
{
    private readonly Mock<IMembershipRepositoryPort> _membershipRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly Mock<IMembershipPlanRepositoryPort> _planRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MembershipService _service;

    public MembershipServiceTests()
    {
        _membershipRepoMock = new Mock<IMembershipRepositoryPort>();
        _clientRepoMock = new Mock<IClientRepository>();
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _planRepoMock = new Mock<IMembershipPlanRepositoryPort>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new MembershipService(
            _membershipRepoMock.Object,
            _clientRepoMock.Object,
            _invoiceServiceMock.Object,
            _planRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task PurchaseMembership_AlreadyHasActive_Should_ThrowException()
    {
        var clientId = 1;
        var planId = 1;

        _clientRepoMock.Setup(r => r.ExistsAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MembershipPlan.Reconstitute(1, "P", 1, 100m));
        _membershipRepoMock.Setup(r => r.HasActiveMembershipForPlanAsync(
                clientId,
                planId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _service.PurchaseMembershipAsync(clientId, planId, PaymentMethod.Card, "Notes");

        await act.Should().ThrowAsync<ActiveMembershipExistsError>();

        _membershipRepoMock.Verify(r => r.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PurchaseMembership_ValidData_Should_AddMembership_And_CreateInvoice()
    {
        var clientId = 1;
        var planId = 1;
        var plan = MembershipPlan.Reconstitute(1, "SixMo", 6, 200m);

        _clientRepoMock.Setup(r => r.ExistsAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _membershipRepoMock.Setup(r => r.HasActiveMembershipForPlanAsync(
                clientId,
                planId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _service.PurchaseMembershipAsync(clientId, planId, PaymentMethod.Card, "Notes");

        _invoiceServiceMock.Verify(s => s.CreateInvoiceAsync(clientId, PaymentMethod.Card, planId, "Notes"), Times.Once);

        _membershipRepoMock.Verify(r => r.AddAsync(It.Is<Membership>(m =>
            m.ClientId == clientId &&
            m.PlanId == planId &&
            m.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
