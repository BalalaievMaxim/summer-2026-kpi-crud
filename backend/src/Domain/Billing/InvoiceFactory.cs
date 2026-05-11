using GymManagement.Domain.Billing.Errors;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Ports;

namespace GymManagement.Domain.Billing;

public class InvoiceFactory
{
    private readonly IClientRepository _clientRepo;
    private readonly IMembershipPlanRepositoryPort _planRepo;

    public InvoiceFactory(IClientRepository clientRepo, IMembershipPlanRepositoryPort planRepo)
    {
        _clientRepo = clientRepo;
        _planRepo = planRepo;
    }

    public async Task<Invoice> CreateForPlanAsync(
        int clientId,
        int membershipPlanId,
        PaymentMethod method,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (clientId <= 0)
            throw new InvalidInvoiceError("ClientId must be a positive number.");

        if (membershipPlanId <= 0)
            throw new InvalidInvoiceError("MembershipPlanId must be a positive number.");

        var plan = await _planRepo.GetMembershipPlanByIdAsync(membershipPlanId, cancellationToken)
            ?? throw new MembershipPlanNotFoundForInvoiceError(membershipPlanId);

        var clientExists = await _clientRepo.ExistsAsync(clientId, cancellationToken);
        if (!clientExists)
            throw new ClientNotFoundForInvoiceError(clientId);

        return Invoice.Create(clientId, plan.Price, method, notes);
    }
}
