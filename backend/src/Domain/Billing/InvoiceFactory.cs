using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Billing;

public sealed class InvoiceFactory
{
    public Invoice Create(Guid clientId, Money amount, string? notes = null)
    {
        return Invoice.Create(clientId, amount, notes);
    }
}
