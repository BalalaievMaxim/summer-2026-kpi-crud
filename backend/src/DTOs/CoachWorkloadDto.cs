namespace GymManagement.DTOs;

public class CoachWorkloadDto
{
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public int TotalClassesScheduled { get; set; }
    public int TotalHoursWorked { get; set; }
    public int AverageClassSize { get; set; }
}
