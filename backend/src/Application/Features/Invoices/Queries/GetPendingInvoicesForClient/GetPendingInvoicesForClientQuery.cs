using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Invoices.Queries.GetPendingInvoicesForClient;

public sealed record GetPendingInvoicesForClientQuery(int ClientId) : IQuery<List<InvoiceResponseDto>>;
