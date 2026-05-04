namespace GymManagement.Application.DTOs;

public record CreateClassRequest(int ClassTypeId, int CoachId, DateTime StartTime, DateTime EndTime, int Capacity);
public record RescheduleRequest(DateTime NewStartTime, DateTime NewEndTime);
