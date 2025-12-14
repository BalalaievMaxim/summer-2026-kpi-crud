using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;

namespace GymManagement.Application.Services;

public class InvoiceService (
    IMembershipPlanRepository membershipPlanRepository,
    IClientRepository clientRepository,
    IInvoiceRepository invoiceRepository,
    IMembershipRepository membershipRepository,
    IUnitOfWork unitOfWork
    ) : IInvoiceService
{
    public async Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes)
    {
        MembershipPlan? plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(membershipPlanId);
        
        if (plan == null)
        {
            throw new Exception($"Membership Plan with ID {membershipPlanId} not found in the database.");
        }
        
        Client? client = await clientRepository.GetClientByIdAsync(clientId);
        
        if (client == null)
        {
            throw new Exception($"Client with ID {clientId} not found in the database.");
        }
        
        var invoice = new Invoice
        {
            ClientId = clientId,
            Client = client,
            Amount = plan.Price,
            Date = DateOnly.FromDateTime(DateTime.Now),
            PaymentMethod = method.ToString().ToLower(),
            Status = nameof(PaymentStatus.Pending).ToLower(), 
            Notes = notes
        };
        
        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();
        
        return invoice;
    }

    public async Task UpdatePaidInvoiceAsync(int clientId, int invoiceId)
    {
        var invoice = await invoiceRepository.GetInvoiceAsync(clientId, invoiceId);
        
        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(clientId); 
        
        var activeMembership = memberships.FirstOrDefault();

        if (invoice != null && invoice.Status == nameof(PaymentStatus.Pending).ToLower())
        {
            await invoiceRepository.MarkAsPayedAsync(invoiceId);
            
            if (activeMembership != null)
            {
                await membershipRepository.MarkAsActiveMembershipAsync(activeMembership.MembershipId);
            }
            await unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<List<Invoice>> GetAllPendingInvoicesAsync(int clientId)
    {
        var pendingInvoices = new List<Invoice>();
        foreach (var invoice in await invoiceRepository.GetAllClientInvoicesAsync(clientId))
        {
            if (invoice.Status == nameof(PaymentStatus.Pending).ToLower()) pendingInvoices.Add(invoice);
        }
        return pendingInvoices;
    }
}