using GymManagement.Core.Enums;

namespace GymManagement.Application.DTOs;

public class CreateInvoiceResponseDto
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public int ClientId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public string? PaymentMethod { get; set; }
    
}