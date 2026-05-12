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

public sealed class ClassRepository(GymManagementContext context) : IClassRepositoryPort
{
    public async Task<DomainClass?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Classes
            .AsNoTracking()
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == id, cancellationToken);

        return entity is null ? null : MapAggregate(entity);
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
