using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class MembershipService(
    IMembershipRepositoryPort membershipRepository,
    IClientRepository clientRepository,
    IInvoiceService invoiceService,
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IUnitOfWork unitOfWork
) : IMembershipService
{
    public async Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes)
    {
        if (!await clientRepository.ExistsAsync(clientId))
            throw new KeyNotFoundException($"Client with ID {clientId} not found.");

        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(planId);
        if (plan is null)
            throw new KeyNotFoundException($"Membership plan with ID {planId} not found.");

        if (await HasClientCertainActiveMembership(clientId, planId))
            throw new InvalidOperationException("Client already has an active membership for this plan.");

        await invoiceService.CreateInvoiceAsync(clientId, method, planId, notes);

        var membership = new MembershipRecord(
            MembershipId: 0,
            ClientId: clientId,
            PlanId: planId,
            StartDate: DateOnly.FromDateTime(DateTime.Now),
            EndDate: DateOnly.FromDateTime(DateTime.Now).AddMonths(plan.DurationMonths),
            IsActive: false);

        await membershipRepository.AddAsync(membership);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<MembershipRecord>> GetActiveMembershipsByClientAsync(int clientId)
    {
        var list = await membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        return list;
    }

    private async Task<bool> HasClientCertainActiveMembership(int clientId, int planId)
    {
        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        return memberships.Any(m => m.PlanId == planId && m.IsActive);
    }
}
