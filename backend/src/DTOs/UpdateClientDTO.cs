namespace GymManagement.DTOs;

public record UpdateClientDto
{
    public required string Email { get; set; }
    public required string Phone { get; set; }
}
