namespace GymManagement.Core.DTOs;

public class CoachEfficiencyDto
{
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public int ClassCount { get; set; }
    public double AverageOccupancyPercent { get; set; }
}
