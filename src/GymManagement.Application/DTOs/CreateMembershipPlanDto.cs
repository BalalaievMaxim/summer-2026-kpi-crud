namespace GymManagement.Application.DTOs;

public record CreateMembershipPlanDto
{
    public string Name { get; set; }
    public int DurationMonth { get; set; }
    public decimal Price { get; set; }
}