using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Domain.Billing;

namespace GymManagement.Application.Features.Memberships.Commands.PurchaseMembership;

public sealed record PurchaseMembershipCommand(
    int ClientId,
    int PlanId,
    PaymentMethod PaymentMethod,
    string? Notes) : ICommand;
