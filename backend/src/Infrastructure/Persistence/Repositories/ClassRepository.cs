using System.Runtime.CompilerServices;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Queries;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Infrastructure.Persistence;
using GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using E = GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class ClassRepository(GymManagementContext context) : IClassScheduleRepository
{
    public async Task<GymClassDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Classes
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<GymClassDetails?> GetByIdWithEnrollmentsAsync(int id, CancellationToken cancellationToken = default)
        => await GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var list = await context.Classes
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .Where(c => c.StartTime >= startOfDay && c.StartTime < endOfDay)
            .OrderBy(c => c.StartTime)
            .ToListAsync(cancellationToken);

        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var list = await context.Classes
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .Where(c => c.StartTime >= startDate && c.StartTime <= endDate)
            .OrderBy(c => c.StartTime)
            .ToListAsync(cancellationToken);

        return list.Select(Map).ToList();
    }

    public async Task<GymClassDetails> CreateAsync(int classTypeId, int coachId, DateTime startUtc, DateTime endUtc, int capacity, CancellationToken cancellationToken = default)
    {
        var entity = new E.Class
        {
            ClassTypeId = classTypeId,
            CoachId = coachId,
            StartTime = startUtc,
            EndTime = endUtc,
            Capacity = capacity
        };

        context.Classes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var loaded = await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .FirstAsync(c => c.ClassId == entity.ClassId, cancellationToken);

        return Map(loaded);
    }

    public async Task<GymClassDetails?> UpdateTimesAsync(int classId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        var existing = await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == classId, cancellationToken);

        if (existing is null)
            return null;

        existing.StartTime = startUtc;
        existing.EndTime = endUtc;

        await context.SaveChangesAsync(cancellationToken);

        return Map(existing);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var classEntity = await context.Classes.FindAsync([id], cancellationToken);
        if (classEntity is null)
            return false;

        context.Classes.Remove(classEntity);
        return true;
    }

    public async Task<bool> HasTimeConflictForCoachAsync(int coachId, DateTime startTime, DateTime endTime, int? excludeClassId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Classes
            .Where(c => c.CoachId == coachId);

        if (excludeClassId.HasValue)
            query = query.Where(c => c.ClassId != excludeClassId.Value);

        return await query.AnyAsync(c =>
            (startTime >= c.StartTime && startTime < c.EndTime) ||
            (endTime > c.StartTime && endTime <= c.EndTime) ||
            (startTime <= c.StartTime && endTime >= c.EndTime), cancellationToken);
    }

    public Task<bool> HasOverlappingClassAsync(int coachId, TimeRange range, int? excludeClassId = null, CancellationToken cancellationToken = default)
    {
        var start = range.Start.UtcDateTime;
        var end = range.End.UtcDateTime;
        return HasTimeConflictForCoachAsync(coachId, start, end, excludeClassId, cancellationToken);
    }

    public async Task<IReadOnlyList<GymClassDetails>> GetClassesByCoachAsync(int coachId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var list = await context.Classes
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .Where(c => c.CoachId == coachId
                        && c.StartTime >= startDate
                        && c.EndTime <= endDate)
            .OrderBy(c => c.StartTime)
            .ToListAsync(cancellationToken);

        return list.Select(Map).ToList();
    }

    public async Task<List<CoachEfficiencyRow>> GetCoachEfficiencyAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sql = @"
            WITH CoachClassStats AS (
                SELECT
                    co.coach_id,
                    co.name AS coach_name,
                    co.specialization,
                    COUNT(c.class_id) AS class_count,
                    SUM(EXTRACT(EPOCH FROM (c.end_time - c.start_time)) / 3600) AS total_hours,
                    AVG(
                        (SELECT COUNT(*) FROM enrollment e WHERE e.class_id = c.class_id)::decimal 
                        / NULLIF(c.capacity, 0) * 100
                    ) AS avg_occupancy_percent
                FROM coach co
                LEFT JOIN class c ON co.coach_id = c.coach_id
                WHERE c.start_time >= {0} AND c.end_time <= {1}
                GROUP BY co.coach_id, co.name, co.specialization
            )
            SELECT
                ccs.coach_id AS ""CoachId"",
                ccs.coach_name AS ""CoachName"",
                ccs.specialization AS ""Specialization"",
                CAST(COALESCE(ccs.total_hours, 0) AS INTEGER) AS ""TotalHours"",
                CAST(COALESCE(ccs.class_count, 0) AS INTEGER) AS ""ClassCount"",
                CAST(COALESCE(ccs.avg_occupancy_percent, 0) AS DECIMAL(5,2)) AS ""AverageOccupancyPercent"",
                CAST(RANK() OVER (ORDER BY ccs.total_hours DESC) AS INTEGER) AS ""CoachRank""
            FROM CoachClassStats ccs
            WHERE ccs.class_count > 0
            ORDER BY ""CoachRank"" ASC, ""TotalHours"" DESC;
        ";

        return await context.Database
            .SqlQuery<CoachEfficiencyRow>(FormattableStringFactory.Create(sql, startDate, endDate))
            .ToListAsync(cancellationToken);
    }

    private static GymClassDetails Map(E.Class c) =>
        new(
            c.ClassId,
            c.ClassTypeId,
            c.ClassType.Name,
            c.CoachId,
            c.Coach.Name,
            c.StartTime,
            c.EndTime,
            c.Capacity,
            c.Enrollments.Select(e => e.ClientId).ToList());
}
