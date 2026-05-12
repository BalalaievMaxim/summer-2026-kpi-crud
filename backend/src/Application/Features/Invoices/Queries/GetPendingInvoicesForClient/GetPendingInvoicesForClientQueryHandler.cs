using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Invoices.Queries.GetPendingInvoicesForClient;

public sealed class GetPendingInvoicesForClientQueryHandler(IInvoiceRepositoryPort invoiceRepository)
    : IQueryHandler<GetPendingInvoicesForClientQuery, List<InvoiceResponseDto>>
{
    public async Task<List<InvoiceResponseDto>> Handle(
        GetPendingInvoicesForClientQuery query,
        CancellationToken cancellationToken = default)
    {
        var invoices = await invoiceRepository.GetPendingInvoicesAsync(query.ClientId, cancellationToken);
        return invoices.Select(ToDto).ToList();
    }

    private static InvoiceResponseDto ToDto(InvoiceRecord invoice) => new()
    {
        InvoiceId = invoice.InvoiceId,
        ClientId = invoice.ClientId,
        Amount = invoice.Amount,
        PaymentMethod = invoice.PaymentMethod ?? "Unknown",
        Status = invoice.Status,
        Date = invoice.Date,
        Notes = invoice.Notes
    };
}
