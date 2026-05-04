namespace GymManagement.Infrastructure.DTOs;

public class ClientActivityDto
{
    public int ClientId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int TotalEnrollments { get; set; }
    public int ClientRank { get; set; } // RANK() OVER window function
}
