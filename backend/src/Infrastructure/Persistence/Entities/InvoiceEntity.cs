namespace GymManagement.Infrastructure.Persistence.Entities;

public class InvoiceEntity
{
    public int InvoiceId { get; set; }
    public int ClientId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UAH";
    public DateOnly Date { get; set; }
    public string Status { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }

    public ClientEntity Client { get; set; } = null!;
}
