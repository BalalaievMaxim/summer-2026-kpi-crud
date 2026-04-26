namespace GymManagement.DTOs;

public record CreateEnrollmentDto
{
    public required int ClientId { get; set; }
    public required int ClassId { get; set; }
}
