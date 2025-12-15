namespace GymManagement.Application.DTOs;

public class CreateMembershipPlanDto
{
    public required string Name { get; set; }
    public int DurationMonth { get; set; }
    public decimal Price { get; set; }
}