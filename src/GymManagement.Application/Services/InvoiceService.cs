using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Core.Exceptions;
using GymManagement.Core.Interfaces;
using GymManagement.Core.DTOs;

namespace GymManagement.Application.Services;

public class InvoiceService(
    IMembershipPlanRepository membershipPlanRepository,
    IClientRepository clientRepository,
    IInvoiceRepository invoiceRepository,
    IMembershipRepository membershipRepository,
    IUnitOfWork unitOfWork
) : IInvoiceService
{
    public async Task<Invoice> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId,
        string? notes)
    {

        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(membershipPlanId);
        if (plan == null)
            throw new NotFoundException($"Membership Plan with ID {membershipPlanId} not found.");

        var client = await clientRepository.GetClientByIdAsync(clientId);
        if (client == null)
            throw new NotFoundException($"Client with ID {clientId} not found.");

        var invoice = new Invoice
        {
            ClientId = clientId,
            Client = client,
            Amount = plan.Price,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = method.ToString().ToLower(),
            Status = nameof(PaymentStatus.Pending).ToLower(),
            Notes = notes
        };

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        return invoice;
    }

    public async Task UpdatePaidInvoiceAsync(int invoiceId)
    {
        var invoice = await invoiceRepository.GetInvoiceAsync(invoiceId);

        if (invoice == null)
            throw new NotFoundException($"Invoice with ID {invoiceId} not found.");

        if (invoice.Status == nameof(PaymentStatus.Paid).ToLower())
            return;

        invoice.Status = nameof(PaymentStatus.Paid).ToLower();

        var memberships = await membershipRepository.GetActiveMembershipsByClientAsync(invoice.ClientId);
        var membership = memberships.FirstOrDefault();

        if (membership != null)
        {
            await membershipRepository.MarkAsActiveMembershipAsync(membership.MembershipId);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<List<Invoice>> GetAllPendingInvoicesAsync(int clientId)
    {

        return await invoiceRepository.GetPendingInvoicesAsync(clientId);
    }
    
    public async Task<List<TotalMembershipRevenueDto>> GetMonthlyRevenueAnalyticsAsync()
    {
        return await invoiceRepository.GetMonthlyRevenueByPlanAsync();
    }
}