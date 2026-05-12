using GymManagement.Application.DTOs;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class InvoiceService(
    IInvoiceRepositoryPort invoiceRepository,
    IInvoiceAnalyticsRepository invoiceAnalyticsRepository,
    IMembershipRepositoryPort membershipRepository,
    InvoiceFactory invoiceFactory,
    IUnitOfWork unitOfWork
) : IInvoiceService
{
    public async Task<InvoiceRecord> CreateInvoiceAsync(
        int clientId,
        PaymentMethod method,
        int membershipPlanId,
        string? notes)
    {
        var invoice = await invoiceFactory.CreateForPlanAsync(
            clientId,
            membershipPlanId,
            method,
            DateOnly.FromDateTime(DateTime.UtcNow),
            notes);

        var newId = await invoiceRepository.AddAsync(invoice);

        return ToRecord(invoice, newId);
    }

    public async Task UpdatePaidInvoiceAsync(int invoiceId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId)
            ?? throw new NotFoundException($"Invoice with ID {invoiceId} not found.");

        if (invoice.IsSettled)
            return;

        invoice.MarkAsPaid();

        await invoiceRepository.UpdateAsync(invoice);

        var membership = await membershipRepository.GetPendingMembershipByClientAsync(invoice.ClientId);
        if (membership is not null)
        {
            membership.Activate();
            await membershipRepository.UpdateAsync(membership);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public Task<List<InvoiceRecord>> GetAllPendingInvoicesAsync(int clientId)
        => invoiceRepository.GetPendingInvoicesAsync(clientId);

    public Task<List<TotalMembershipRevenueRow>> GetMonthlyRevenueAnalyticsAsync()
        => invoiceAnalyticsRepository.GetMonthlyRevenueByPlanAsync();

    private static InvoiceRecord ToRecord(Invoice invoice, int id) =>
        new(
            InvoiceId: id,
            ClientId: invoice.ClientId,
            Amount: invoice.Amount,
            Date: invoice.Date,
            Status: invoice.Status.ToString().ToLowerInvariant(),
            PaymentMethod: invoice.Method.ToString().ToLowerInvariant(),
            Notes: invoice.Notes);
}
