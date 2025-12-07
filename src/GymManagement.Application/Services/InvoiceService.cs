using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Infrastructure.Repositories;

namespace GymManagement.Application.Services;

public class InvoiceService (MembershipRepository membershipRepository, ClientRepository clientRepository)
{
    public async Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId, string? notes)
    {
        Invoice invoice = new Invoice();
        Membershipplan plan = await membershipRepository.GetMembershipPlanByIdAsync(membershipPlanId);
        
        var client = await clientRepository.GetClientByIdAsync(clientId);
        if ( client != null) invoice.Client = client;
        
        invoice.Amount = plan.Price;
        invoice.ClientId = clientId;
        invoice.Date = DateOnly.FromDateTime(DateTime.Now);
        invoice.PaymentMethod = nameof(method).ToLower();
        invoice.Status = nameof(PaymentStatus.Pending).ToLower();

        if (notes != null) invoice.Notes = notes;
        
        return invoice;
    }
    
}