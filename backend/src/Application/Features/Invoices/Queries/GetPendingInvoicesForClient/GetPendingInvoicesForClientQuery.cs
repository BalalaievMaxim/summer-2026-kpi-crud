using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Billing;

namespace GymManagement.Application.Features.Invoices.Queries.GetPendingInvoicesForClient;

public sealed record GetPendingInvoicesForClientQuery(int ClientId) : IQuery<List<InvoiceRecord>>;
