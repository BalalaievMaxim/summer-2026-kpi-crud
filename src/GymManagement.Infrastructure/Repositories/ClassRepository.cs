using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Repositories;

public class ClassRepository(GymManagementContext context) : IClassRepository
{
    public async Task<Class?> GetByIdAsync(int id)
    {
        return await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Client)
            .FirstOrDefaultAsync(c => c.ClassId == id);
    }
    
    public async Task<Class?> GetByIdWithEnrollmentsAsync(int classId)
    {
        return await context.Classes
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }

    public async Task<IEnumerable<Class>> GetAllAsync()
    {
        return await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<Class> CreateAsync(Class classEntity)
    {
        context.Classes.Add(classEntity);
        await context.SaveChangesAsync();
        
        // return include for full inf
        return (await GetByIdAsync(classEntity.ClassId))!;
    }

    public async Task<Class?> UpdateAsync(Class classEntity)
    {
        var existing = await context.Classes.FindAsync(classEntity.ClassId);
        if (existing == null)
        {
            return null;
        }

        existing.ClassTypeId = classEntity.ClassTypeId;
        existing.CoachId = classEntity.CoachId;
        existing.StartTime = classEntity.StartTime;
        existing.EndTime = classEntity.EndTime;
        existing.Capacity = classEntity.Capacity;

        await context.SaveChangesAsync();
        
        return await GetByIdAsync(existing.ClassId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var classEntity = await context.Classes.FindAsync(id);
        if (classEntity == null)
        {
            return false;
        }

        context.Classes.Remove(classEntity);
        await context.SaveChangesAsync();
        return true;
    }

    // special methods for my domen

    public async Task<IEnumerable<Class>> GetScheduleForDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Where(c => c.StartTime >= startOfDay && c.StartTime < endOfDay)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetScheduleForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Where(c => c.StartTime >= startDate && c.StartTime <= endDate)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }


    //for validation! time conflicts
    public async Task<bool> HasTimeConflictForCoachAsync(
        int coachId, 
        DateTime startTime, 
        DateTime endTime, 
        int? excludeClassId = null)
    {
        var query = context.Classes
            .Where(c => c.CoachId == coachId);

        if (excludeClassId.HasValue)
        {
            query = query.Where(c => c.ClassId != excludeClassId.Value);
        }

        return await query.AnyAsync(c =>
            (startTime >= c.StartTime && startTime < c.EndTime) ||
            (endTime > c.StartTime && endTime <= c.EndTime) ||
            (startTime <= c.StartTime && endTime >= c.EndTime)
        );
    }

    public async Task<IEnumerable<Class>> GetUpcomingClassesByCoachAsync(int coachId)
    {
        var now = DateTime.UtcNow;
        
        return await context.Classes
            .Include(c => c.ClassType)
            .Where(c => c.CoachId == coachId && c.StartTime > now)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetClassesByCoachAsync(int coachId, DateTime startDate, DateTime endDate)
    {
        return await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Enrollments)
            .Where(c => c.CoachId == coachId 
                && c.StartTime >= startDate 
                && c.EndTime <= endDate)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }
}
