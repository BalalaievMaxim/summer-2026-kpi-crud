using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Repositories;

namespace GymManagement.Application.Services;

public class InvoiceService (IMembershipPlanRepository membershipPlanRepository, IClientRepository clientRepository, IInvoiceRepository invoiceRepository, IMembershipRepository membershipRepository, IUnitOfWork unitOfWork)
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
            invoice.PaymentMethod = method.ToString().ToLower();
            invoice.Status = nameof(PaymentStatus.Pending).ToLower();
        }

        if (notes != null) invoice.Notes = notes;
        
        return invoice;
    }

    public async Task UpdatePaidInvoiceAsync(int clientId, int invoiceId)
    {
        var invoice = await invoiceRepository.GetInvoiceAsync(clientId, invoiceId);
        var membership = membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        if (invoice != null && invoice.Status == nameof(PaymentStatus.Pending).ToLower())
        {
            await invoiceRepository.MarkAsPayedAsync(invoiceId);
            await membershipRepository.MarkAsActiveMembershipAsync(membership.Id);
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