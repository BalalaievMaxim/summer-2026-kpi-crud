using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IClassService
{
    Task<GymClassDetails> CreateClassAsync(int classTypeId, int coachId, DateTime startTime, DateTime endTime, int capacity);
    Task<bool> DeleteClassAsync(int classId);
    Task<GymClassDetails?> UpdateClassAsync(int classId, DateTime newStartTime, DateTime newEndTime);
    Task<GymClassDetails?> GetClassByIdAsync(int id);
    Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateAsync(DateTime date);
    Task<IReadOnlyList<GymClassDetails>> GetScheduleForWeekAsync(DateTime startOfWeek);
    Task<IReadOnlyList<ClassAttendanceRow>> GetClassAttendanceAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<CoachWorkloadRow> GetCoachWorkloadAsync(int coachId, DateTime startDate, DateTime endDate);
    Task<List<CoachEfficiencyRow>> GetCoachEfficiencyAnalyticsAsync(DateTime startDate, DateTime endDate);
}
