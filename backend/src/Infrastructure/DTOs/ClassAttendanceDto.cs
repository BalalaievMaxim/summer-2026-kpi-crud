namespace GymManagement.Infrastructure.DTOs;

public class ClassAttendanceDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public decimal OccupancyRate { get; set; }
}
