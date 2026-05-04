using GymManagement.Infrastructure.Persistence.Entities;
using System.Threading.Tasks;
using GymManagement.Infrastructure.DTOs;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(int id);
    Task<Class?> GetByIdWithEnrollmentsAsync(int classId);
    Task<IEnumerable<Class>> GetAllAsync();
    Task<Class> CreateAsync(Class classEntity);
    Task<Class?> UpdateAsync(Class classEntity);
    Task<bool> DeleteAsync(int id);
    
    Task<IEnumerable<Class>> GetScheduleForDateAsync(DateTime date);
    Task<IEnumerable<Class>> GetScheduleForDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> HasTimeConflictForCoachAsync(int coachId, DateTime startTime, DateTime endTime, int? excludeClassId = null);
    Task<IEnumerable<Class>> GetUpcomingClassesByCoachAsync(int coachId);
    Task<IEnumerable<Class>> GetClassesByCoachAsync(int coachId, DateTime startDate, DateTime endDate);
    Task<List<CoachEfficiencyDto>> GetCoachEfficiencyAnalyticsAsync(DateTime startDate, DateTime endDate);
}
