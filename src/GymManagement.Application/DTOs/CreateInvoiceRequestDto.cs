using System.ComponentModel.DataAnnotations;
using GymManagement.Core.Enums;

namespace GymManagement.Application.DTOs;

public record CreateInvoiceRequestDto
{
    [Required(ErrorMessage = "ClientId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ClientId must be positive.")]
    public int ClientId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    
    [Required(ErrorMessage = "MembershipPlanId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "MembershipPlanId must be positive.")]
    public int MembershipPlanId { get; set; }
    public string? Notes { get; set; }
}