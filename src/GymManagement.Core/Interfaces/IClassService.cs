using GymManagement.Core.Entities;
using GymManagement.Core.DTOs;

namespace GymManagement.Core.Interfaces;

public interface IClassService
{
    Task<Class> CreateClassAsync(int classTypeId, int coachId, DateTime startTime, DateTime endTime, int capacity);
    Task<bool> DeleteClassAsync(int classId);
    Task<Class?> UpdateClassAsync(int classId, DateTime newStartTime, DateTime newEndTime);
    Task<IEnumerable<Class>> GetScheduleForDateAsync(DateTime date);
    Task<IEnumerable<Class>> GetScheduleForWeekAsync(DateTime startOfWeek);
    Task<IEnumerable<ClassAttendanceDto>> GetClassAttendanceAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<CoachWorkloadDto> GetCoachWorkloadAsync(int coachId, DateTime startDate, DateTime endDate);
}

