namespace GymManagement.Domain.Billing;

public sealed record InvoiceRecord(
    int InvoiceId,
    int ClientId,
    decimal Amount,
    DateOnly Date,
    string Status,
    string PaymentMethod,
    string? Notes);
