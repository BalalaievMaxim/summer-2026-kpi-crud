using System.ComponentModel.DataAnnotations;
using GymManagement.Domain.Billing;

namespace GymManagement.Application.DTOs;

public class CreateInvoiceRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ClientId must be valid.")]
    public int ClientId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MembershipPlanId must be valid.")]
    public int MembershipPlanId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class InvoiceResponseDto
{
    public int InvoiceId { get; set; }
    public int ClientId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
}
