using System.Runtime.CompilerServices;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainClass = GymManagement.Domain.Classes.Class;
using DomainEnrollment = GymManagement.Domain.Enrollments.Enrollment;
using E = GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class ClassRepository(GymManagementContext context) : IClassRepositoryPort, IClassScheduleRepository
{
    public async Task<DomainClass?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Classes
            .AsNoTracking()
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == id, cancellationToken);

        return entity is null ? null : MapAggregate(entity);
    }

    public async Task<GymClassDetails?> GetClassByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Classes
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

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

    public async Task<int> AddAsync(DomainClass classEntity, CancellationToken cancellationToken = default)
    {
        var entity = new E.Class
        {
            ClassTypeId = classEntity.ClassTypeId,
            CoachId = classEntity.CoachId,
            StartTime = classEntity.Schedule.Start.UtcDateTime,
            EndTime = classEntity.Schedule.End.UtcDateTime,
            Capacity = classEntity.Capacity
        };

        context.Classes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.ClassId;
    }

    public async Task UpdateAsync(DomainClass classEntity, CancellationToken cancellationToken = default)
    {
        var existing = await context.Classes
            .Include(c => c.ClassType)
            .Include(c => c.Coach)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == classEntity.Id, cancellationToken);

        if (existing is null)
            return;

        existing.ClassTypeId = classEntity.ClassTypeId;
        existing.CoachId = classEntity.CoachId;
        existing.StartTime = classEntity.Schedule.Start.UtcDateTime;
        existing.EndTime = classEntity.Schedule.End.UtcDateTime;
        existing.Capacity = classEntity.Capacity;

        SyncEnrollments(existing, classEntity.Enrollments);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var classEntity = await context.Classes.FindAsync([id], cancellationToken);
        if (classEntity is null)
            return false;

        context.Classes.Remove(classEntity);
        return true;
    }

    private async Task<bool> HasTimeConflictForCoachAsync(int coachId, DateTime startTime, DateTime endTime, int? excludeClassId = null, CancellationToken cancellationToken = default)
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

    private static DomainClass MapAggregate(E.Class c)
    {
        var schedule = TimeRange.Create(
            new DateTimeOffset(DateTime.SpecifyKind(c.StartTime, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(c.EndTime, DateTimeKind.Utc)));

        var enrollments = c.Enrollments.Select(e =>
            DomainEnrollment.Reconstitute(
                e.EnrollmentId,
                e.ClientId,
                e.ClassId,
                new DateTimeOffset(DateTime.SpecifyKind(e.RegistrationTime, DateTimeKind.Utc))));

        return DomainClass.Reconstitute(c.ClassId, c.ClassTypeId, c.CoachId, schedule, c.Capacity, enrollments);
    }

    private static void SyncEnrollments(E.Class existing, IReadOnlyList<DomainEnrollment> enrollments)
    {
        var desiredClientIds = enrollments.Select(e => e.ClientId).ToHashSet();

        foreach (var removed in existing.Enrollments.Where(e => !desiredClientIds.Contains(e.ClientId)).ToList())
            existing.Enrollments.Remove(removed);

        foreach (var enrollment in enrollments.Where(e => existing.Enrollments.All(existingEnrollment =>
                     existingEnrollment.ClientId != e.ClientId)))
        {
            existing.Enrollments.Add(new E.Enrollment
            {
                ClientId = enrollment.ClientId,
                ClassId = existing.ClassId,
                RegistrationTime = enrollment.RegistrationTime.UtcDateTime
            });
        }
    }
}
