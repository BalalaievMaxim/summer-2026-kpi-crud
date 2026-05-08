using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Domain.Billing.Errors;

namespace GymManagement.Domain.Billing;

public sealed class Invoice : AggregateRoot<Guid>
{
    public Guid ClientId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTimeOffset Date { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod? Method { get; private set; }
    public string? Notes { get; private set; }

    private Invoice() { }

    private Invoice(Guid id, Guid clientId, Money amount, string? notes)
        : base(id)
    {
        ClientId = clientId;
        Amount = amount;
        Date = DateTimeOffset.UtcNow;
        Status = PaymentStatus.Pending;
        Method = null;
        Notes = notes;
    }

    public static Invoice Create(Guid clientId, Money amount, string? notes = null)
    {
        if (clientId == Guid.Empty)
            throw new InvalidInvoiceError("ClientId cannot be empty.");
        if (amount.Amount <= 0)
            throw new InvalidInvoiceError("Invoice amount must be greater than zero.");

        return new Invoice(Guid.NewGuid(), clientId, amount, notes);
    }

    public bool IsSettled => Status == PaymentStatus.Paid;

    public void MarkAsPaid(PaymentMethod method)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidInvoiceOperationError($"Cannot mark invoice as paid. Current status: {Status}.");

        Status = PaymentStatus.Paid;
        Method = method;
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Paid)
            throw new InvalidInvoiceOperationError("Cannot cancel an already paid invoice.");
        if (Status == PaymentStatus.Cancelled)
            throw new InvalidInvoiceOperationError("Invoice is already cancelled.");

        Status = PaymentStatus.Cancelled;
    }

    public void MarkAsOverdue()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidInvoiceOperationError($"Cannot mark as overdue. Current status: {Status}.");

        Status = PaymentStatus.Overdue;
    }
}
