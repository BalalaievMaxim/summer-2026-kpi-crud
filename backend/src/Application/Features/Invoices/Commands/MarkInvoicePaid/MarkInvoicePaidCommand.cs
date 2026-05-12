using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Invoices.Commands.MarkInvoicePaid;

public sealed record MarkInvoicePaidCommand(int InvoiceId) : ICommand;
