using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Infrastructure.Repositories;

namespace GymManagement.Application.Services;

public class MembershipService (
    MembershipRepository membershipRepository,
    ClientRepository clientRepository,
    InvoiceService invoiceService,
    MembershipPlanRepository membershipPlanRepository,
    InvoiceRepository invoiceRepository,
    UnitOfWork unitOfWork
    )
{

    public async Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes)
    {
        Membershipplan? plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(planId);
        Membership membership = new Membership();
        
        
        
        var client = await clientRepository.GetClientByIdAsync(clientId);
        if ( plan!= null && client != null && membershipPlanRepository.GetMembershipPlanByIdAsync(planId).Result != null && await HasClientCertainActiveMembership(clientId,planId))
        {
            membership.PlanId = planId;
            membership.ClientId = clientId;
            membership.StartDate  = DateOnly.FromDateTime(DateTime.Now);
            membership.EndDate = membership.StartDate.AddMonths(plan.DurationMonths);
            membership.IsActive = false;
            membership.Plan = plan;
            membership.Client = client;
        }

        Invoice invoice = await invoiceService.CreateInvoiceAsync(clientId, method, planId, notes);

        await invoiceRepository.AddAsync(invoice);
        await membershipRepository.AddAsync(membership);
        await unitOfWork.SaveChangesAsync();

    }

    private async Task<bool> HasClientCertainActiveMembership(int clientId, int planId)
    {
        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        bool hasCertainActiveMembership = false;
        foreach (var membership in memberships)
        {
            if (membership.PlanId == planId && membership.IsActive == true) hasCertainActiveMembership = true;
        }
        return hasCertainActiveMembership;
    }
}