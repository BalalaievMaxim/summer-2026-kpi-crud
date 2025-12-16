using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly GymManagementContext _context;

    public ClassRepository(GymManagementContext context)
    {
        _context = context;
    }

    public async Task<Class?> GetByIdAsync(int id)
    {
        return await _context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Client)
            .FirstOrDefaultAsync(c => c.ClassId == id);
    }

    public async Task<Class?> GetByIdWithEnrollmentsAsync(int id)
    {
        return await _context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Client)
            .FirstOrDefaultAsync(c => c.ClassId == id);
    }

    public async Task<IEnumerable<Class>> GetAllAsync()
    {
        return await _context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<Class> CreateAsync(Class classEntity)
    {
        _context.Classes.Add(classEntity);
        await _context.SaveChangesAsync();
        
        return (await GetByIdAsync(classEntity.ClassId))!;
    }

    public async Task<Class?> UpdateAsync(Class classEntity)
    {
        var existing = await _context.Classes.FindAsync(classEntity.ClassId);
        if (existing == null)
        {
            return null;
        }

        existing.ClassTypeId = classEntity.ClassTypeId;
        existing.CoachId = classEntity.CoachId;
        existing.StartTime = classEntity.StartTime;
        existing.EndTime = classEntity.EndTime;
        existing.Capacity = classEntity.Capacity;

        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(existing.ClassId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var classEntity = await _context.Classes.FindAsync(id);
        if (classEntity == null)
        {
            return false;
        }

        _context.Classes.Remove(classEntity);
        return true;
    }

    public async Task<IEnumerable<Class>> GetScheduleForDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Where(c => c.StartTime >= startOfDay && c.StartTime < endOfDay)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetScheduleForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Where(c => c.StartTime >= startDate && c.StartTime <= endDate)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasTimeConflictForCoachAsync(
        int coachId, 
        DateTime startTime, 
        DateTime endTime, 
        int? excludeClassId = null)
    {
        var query = _context.Classes
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
        
        return await _context.Classes
            .Include(c => c.ClassType)
            .Where(c => c.CoachId == coachId && c.StartTime > now)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetClassesByCoachAsync(int coachId, DateTime startDate, DateTime endDate)
    {
        return await _context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Enrollments)
            .Where(c => c.CoachId == coachId 
                && c.StartTime >= startDate 
                && c.EndTime <= endDate)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }
}
