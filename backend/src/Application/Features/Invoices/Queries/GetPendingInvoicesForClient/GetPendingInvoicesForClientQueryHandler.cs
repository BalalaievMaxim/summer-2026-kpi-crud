using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Invoices.Queries.GetPendingInvoicesForClient;

public sealed class GetPendingInvoicesForClientQueryHandler(IInvoiceRepositoryPort invoiceRepository)
    : IQueryHandler<GetPendingInvoicesForClientQuery, List<InvoiceRecord>>
{
    public Task<List<InvoiceRecord>> Handle(
        GetPendingInvoicesForClientQuery query,
        CancellationToken cancellationToken = default)
        => invoiceRepository.GetPendingInvoicesAsync(query.ClientId, cancellationToken);
}
