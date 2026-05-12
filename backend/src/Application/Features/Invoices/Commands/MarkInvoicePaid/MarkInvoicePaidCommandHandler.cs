using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Exceptions;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Invoices.Commands.MarkInvoicePaid;

public sealed class MarkInvoicePaidCommandHandler(
    IInvoiceRepositoryPort invoiceRepository,
    IMembershipRepositoryPort membershipRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<MarkInvoicePaidCommand>
{
    public async Task Handle(MarkInvoicePaidCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice with ID {command.InvoiceId} not found.");

        if (invoice.IsSettled)
            return;

        invoice.MarkAsPaid();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        var membership = await membershipRepository.GetPendingMembershipByClientAsync(invoice.ClientId, cancellationToken);
        if (membership is not null)
        {
            membership.Activate();
            await membershipRepository.UpdateAsync(membership, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
