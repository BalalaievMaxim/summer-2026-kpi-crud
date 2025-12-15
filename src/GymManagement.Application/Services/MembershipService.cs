using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;

namespace GymManagement.Application.Services;

public class MembershipService(
    IMembershipRepository membershipRepository,
    IClientRepository clientRepository,
    IInvoiceService invoiceService,
    IMembershipPlanRepository membershipPlanRepository,
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork
    ) : IMembershipService
{
    public async Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes)
    {
        var client = await clientRepository.GetClientByIdAsync(clientId);
        if (client == null)
            throw new KeyNotFoundException($"Client with ID {clientId} not found.");

        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(planId);
        if (plan == null)
            throw new KeyNotFoundException($"Membership plan with ID {planId} not found.");

        bool alreadyHasActive = await HasClientCertainActiveMembership(clientId, planId);
        if (alreadyHasActive)
            throw new InvalidOperationException("Client already has an active membership for this plan.");

        var membership = new Membership
        {
            ClientId = clientId,
            PlanId = planId,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(plan.DurationMonths),
            IsActive = false 
        };

        var invoice = await invoiceService.CreateInvoiceAsync(clientId, method, planId, notes);
        
        await invoiceRepository.AddAsync(invoice);
        await membershipRepository.AddAsync(membership);
        
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> HasClientCertainActiveMembership(int clientId, int planId)
    {
        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        return memberships.Any(m => m.PlanId == planId && m.IsActive == true);
    }
}