using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Entities.Enums;
using GymManagement.Application.Exceptions;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;

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

        var membership = await membershipRepository.GetPendingMembershipByClientAsync(invoice.ClientId);

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