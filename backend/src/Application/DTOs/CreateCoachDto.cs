namespace GymManagement.Application.DTOs;

public class CreateCoachDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
