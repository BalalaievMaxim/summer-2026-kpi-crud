namespace GymManagement.Infrastructure.DTOs;

public class TotalMembershipRevenueDto
{
    public required string RevenueMonth { get; set; }
    public required string PlanName { get; set; }
    public required decimal TotalRevenue { get; set; }
}