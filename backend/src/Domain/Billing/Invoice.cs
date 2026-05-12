using GymManagement.Domain.Billing.Errors;
using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Billing;

public sealed class Invoice : AggregateRoot<int>
{
    public int ClientId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Notes { get; private set; }

    private Invoice() { }

    private Invoice(
        int id,
        int clientId,
        decimal amount,
        DateOnly date,
        PaymentStatus status,
        PaymentMethod method,
        string? notes) : base(id)
    {
        ClientId = clientId;
        Amount = amount;
        Date = date;
        Status = status;
        Method = method;
        Notes = notes;
    }

    public static Invoice Create(int clientId, decimal amount, PaymentMethod method, DateOnly date, string? notes = null)
    {
        if (clientId <= 0)
            throw new InvalidInvoiceError("ClientId must be a positive number.");
        if (amount <= 0)
            throw new InvalidInvoiceError("Invoice amount must be greater than zero.");

        return new Invoice(
            id: 0,
            clientId: clientId,
            amount: amount,
            date: date,
            status: PaymentStatus.Pending,
            method: method,
            notes: notes);
    }

    public static Invoice Reconstitute(
        int id,
        int clientId,
        decimal amount,
        DateOnly date,
        PaymentStatus status,
        PaymentMethod method,
        string? notes)
        => new(id, clientId, amount, date, status, method, notes);

    public bool IsSettled => Status == PaymentStatus.Paid;

    public void MarkAsPaid()
    {
        if (Status == PaymentStatus.Paid)
            return;

        if (Status != PaymentStatus.Pending)
            throw new InvalidInvoiceOperationError($"Cannot mark invoice as paid. Current status: {Status}.");

        Status = PaymentStatus.Paid;
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
