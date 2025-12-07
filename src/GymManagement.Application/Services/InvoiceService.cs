using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Infrastructure.Repositories;

namespace GymManagement.Application.Services;

public class InvoiceService (MembershipPlanRepository membershipPlanRepository, ClientRepository clientRepository, InvoiceRepository invoiceRepository, MembershipRepository membershipRepository)
{
    public async Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes)
    {
        Invoice invoice = new Invoice();
        Membershipplan? plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(membershipPlanId);
        
        var client = await clientRepository.GetClientByIdAsync(clientId);
        if ( client != null && plan != null)
        {
            invoice.Client = client;
            invoice.Amount = plan.Price;
            invoice.ClientId = clientId;
            invoice.Date = DateOnly.FromDateTime(DateTime.Now);
            invoice.PaymentMethod = nameof(method).ToLower();
            invoice.Status = nameof(PaymentStatus.Pending).ToLower();
        }

        if (notes != null) invoice.Notes = notes;
        
        return invoice;
    }

    public async Task UpdatePayedInvoiceAsync(int clientId, int invoiceId)
    {
        var invoice = await invoiceRepository.GetInvoiceAsync(clientId, invoiceId);
        var membership = membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        if (invoice != null && invoice.Status == nameof(PaymentStatus.Pending).ToLower())
        {
            await invoiceRepository.MarkAsPayedAsync(invoiceId);
            await membershipRepository.MarkAsActiveMembershipAsync(membership.Id);
        }
    } 
    
}