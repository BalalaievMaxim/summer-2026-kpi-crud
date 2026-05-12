using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Memberships.Commands.PurchaseMembership;

public sealed class PurchaseMembershipCommandHandler(
    IMembershipRepositoryPort membershipRepository,
    IClientRepository clientRepository,
    IInvoiceRepositoryPort invoiceRepository,
    IMembershipPlanRepositoryPort membershipPlanRepository,
    InvoiceFactory invoiceFactory,
    IUnitOfWork unitOfWork) : ICommandHandler<PurchaseMembershipCommand>
{
    public async Task Handle(PurchaseMembershipCommand command, CancellationToken cancellationToken = default)
    {
        if (!await clientRepository.ExistsAsync(command.ClientId, cancellationToken))
            throw new ClientNotFoundError(command.ClientId);

        var plan = await membershipPlanRepository.GetByIdAsync(command.PlanId, cancellationToken);
        if (plan is null)
            throw new MembershipPlanNotFoundError(command.PlanId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await membershipRepository.HasActiveMembershipForPlanAsync(command.ClientId, command.PlanId, today, cancellationToken))
            throw new ActiveMembershipExistsError(command.ClientId, command.PlanId);

        var invoice = await invoiceFactory.CreateForPlanAsync(
            command.ClientId,
            command.PlanId,
            command.PaymentMethod,
            today,
            command.Notes,
            cancellationToken);

        invoiceRepository.Stage(invoice);

        var membership = Membership.PurchasePending(command.ClientId, plan, today);
        await membershipRepository.AddAsync(membership, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
