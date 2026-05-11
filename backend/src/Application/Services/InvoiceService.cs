using GymManagement.Application.Exceptions;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Queries;

namespace GymManagement.Application.Services;

public sealed class InvoiceService(
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IClientRepository clientRepository,
    IInvoiceRepositoryPort invoiceRepository,
    IMembershipRepositoryPort membershipRepository,
    IUnitOfWork unitOfWork
) : IInvoiceService
{
    public async Task<InvoiceRecord> CreateInvoiceAsync(int clientId, PaymentMethod method, int membershipPlanId,
        string? notes)
    {
        var plan = await membershipPlanRepository.GetMembershipPlanByIdAsync(membershipPlanId);
        if (plan is null)
            throw new NotFoundException($"Membership Plan with ID {membershipPlanId} not found.");

        if (!await clientRepository.ExistsAsync(clientId))
            throw new NotFoundException($"Client with ID {clientId} not found.");

        var invoice = new InvoiceRecord(
            InvoiceId: 0,
            ClientId: clientId,
            Amount: plan.Price,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod: method.ToString().ToLowerInvariant(),
            Status: nameof(PaymentStatus.Pending).ToLowerInvariant(),
            Notes: notes);

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var pending = await invoiceRepository.GetPendingInvoicesAsync(clientId);
        var created = pending.OrderByDescending(i => i.InvoiceId).FirstOrDefault();
        return created ?? throw new InvalidOperationException("Invoice was not persisted.");
    }

    public async Task UpdatePaidInvoiceAsync(int invoiceId)
    {
        var invoice = await invoiceRepository.GetInvoiceAsync(invoiceId);

        if (invoice is null)
            throw new NotFoundException($"Invoice with ID {invoiceId} not found.");

        if (invoice.Status == nameof(PaymentStatus.Paid).ToLowerInvariant())
            return;

        await invoiceRepository.UpdateStatusAsync(invoiceId, nameof(PaymentStatus.Paid).ToLowerInvariant());

        var membership = await membershipRepository.GetPendingMembershipByClientAsync(invoice.ClientId);

        if (membership is not null)
            await membershipRepository.MarkAsActiveMembershipAsync(membership.MembershipId);
    }

    public Task<List<InvoiceRecord>> GetAllPendingInvoicesAsync(int clientId)
        => invoiceRepository.GetPendingInvoicesAsync(clientId);

    public Task<List<TotalMembershipRevenueRow>> GetMonthlyRevenueAnalyticsAsync()
        => invoiceRepository.GetMonthlyRevenueByPlanAsync();
}
