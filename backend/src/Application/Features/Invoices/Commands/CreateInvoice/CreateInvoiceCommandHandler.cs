using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(
    IInvoiceRepositoryPort invoiceRepository,
    InvoiceFactory invoiceFactory) : ICommandHandler<CreateInvoiceCommand, int>
{
    public async Task<int> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceFactory.CreateForPlanAsync(
            command.ClientId,
            command.MembershipPlanId,
            command.PaymentMethod,
            DateOnly.FromDateTime(DateTime.UtcNow),
            command.Notes,
            cancellationToken);

        return await invoiceRepository.AddAsync(invoice, cancellationToken);
    }
}
