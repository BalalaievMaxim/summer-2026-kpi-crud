using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Billing;

namespace GymManagement.Application.Features.Invoices.Commands.CreateInvoice;

public sealed record CreateInvoiceCommand(
    int ClientId,
    PaymentMethod PaymentMethod,
    int MembershipPlanId,
    string? Notes) : ICommand<int>;
