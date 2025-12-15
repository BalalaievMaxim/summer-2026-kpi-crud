using GymManagement.Core.Enums;

namespace GymManagement.Application.DTOs;

public class PurchaseMembershipDto
{
    public int ClientId { get; set; }
    public int PlanId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}