using DomainInvoice = GymManagement.Domain.Billing.Invoice;
using InvoiceEntity = GymManagement.Infrastructure.Persistence.Entities.Invoice;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Infrastructure.Persistence.Mappers;

public static class InvoiceMapper
{
    public static DomainInvoice ToDomain(InvoiceEntity e)
    {
        var clientId = IntToGuid(e.ClientId);
        var amount = Money.Create(e.Amount, "UAH");

        var invoice = DomainInvoice.Create(clientId, amount, e.Notes);

        if (e.Status == "Paid" && e.PaymentMethod is not null)
        {
            var method = Enum.Parse<PaymentMethod>(e.PaymentMethod);
            invoice.MarkAsPaid(method);
        }
        else if (e.Status == "Cancelled")
        {
            invoice.Cancel();
        }
        else if (e.Status == "Overdue")
        {
            invoice.MarkAsOverdue();
        }

        return invoice;
    }

    public static InvoiceEntity ToEntity(DomainInvoice d)
    {
        return new InvoiceEntity
        {
            InvoiceId = GuidToInt(d.Id),
            ClientId = GuidToInt(d.ClientId),
            Amount = d.Amount.Amount,
            PaymentMethod = d.Method?.ToString(),
            Notes = d.Notes
        };
    }

    private static Guid IntToGuid(int id) => new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
