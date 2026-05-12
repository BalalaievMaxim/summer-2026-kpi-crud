using GymManagement.Application.Services.Interfaces;
using GymManagement.Application.DTOs;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Application.Services;

public sealed class ClassService(
    IClassRepositoryPort classRepository,
    IClassScheduleRepository classScheduleRepository,
    ICoachRepository coachRepository,
    IClassTypeRepositoryPort classTypeRepository,
    ClassFactory classFactory,
    IUnitOfWork unitOfWork) : IClassService
{
    public async Task<GymClassDetails> CreateClassAsync(
        int classTypeId,
        int coachId,
        DateTime startTime,
        DateTime endTime,
        int capacity)
    {
        if (!await classTypeRepository.ExistsAsync(classTypeId))
            throw new Application.Exceptions.NotFoundException($"ClassType with ID {classTypeId} not found.");

        var classEntity = await classFactory.CreateAsync(
            classTypeId,
            coachId,
            new DateTimeOffset(DateTime.SpecifyKind(startTime, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(endTime, DateTimeKind.Utc)),
            capacity,
            DateTimeOffset.UtcNow);

        var classId = await classRepository.AddAsync(classEntity);
        return await classScheduleRepository.GetClassByIdAsync(classId)
            ?? throw new Application.Exceptions.NotFoundException($"Class with ID {classId} not found.");
    }

    public async Task<GymClassDetails?> UpdateClassAsync(int classId, DateTime newStartTime, DateTime newEndTime)
    {
        var classEntity = await classRepository.GetByIdAsync(classId);
        if (classEntity is null)
            return null;

        if (classEntity.Schedule.Start < DateTimeOffset.UtcNow)
            throw new ClassInPastError();

        var newRange = TimeRange.Create(
            new DateTimeOffset(DateTime.SpecifyKind(newStartTime, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(newEndTime, DateTimeKind.Utc)));

        var hasConflict = await classRepository.HasOverlappingClassAsync(
            classEntity.CoachId,
            newRange,
            classId);

        classEntity.Reschedule(newRange, hasConflict, DateTimeOffset.UtcNow);

        await classRepository.UpdateAsync(classEntity);
        await unitOfWork.SaveChangesAsync();
        return await classScheduleRepository.GetClassByIdAsync(classId);
    }

    public async Task<bool> DeleteClassAsync(int classId)
    {
        var classEntity = await classRepository.GetByIdAsync(classId);
        if (classEntity is null)
            return false;

        classEntity.EnsureCanBeDeleted();

        await classRepository.DeleteAsync(classId);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task<GymClassDetails?> GetClassByIdAsync(int id)
        => classScheduleRepository.GetClassByIdAsync(id);

    public Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateAsync(DateTime date)
        => classScheduleRepository.GetScheduleForDateAsync(date);

    public async Task<IReadOnlyList<GymClassDetails>> GetScheduleForWeekAsync(DateTime startOfWeek)
    {
        var endOfWeek = startOfWeek.AddDays(7);
        return await classScheduleRepository.GetScheduleForDateRangeAsync(startOfWeek, endOfWeek);
    }

    public async Task<IReadOnlyList<ClassAttendanceRow>> GetClassAttendanceAnalyticsAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var classes = await classScheduleRepository.GetScheduleForDateRangeAsync(startDate, endDate);

        return classes
            .Select(c => new ClassAttendanceRow(
                c.ClassId,
                c.ClassTypeName,
                c.CoachName,
                c.StartTimeUtc,
                c.Capacity,
                c.EnrollmentClientIds.Count,
                c.Capacity > 0
                    ? (decimal)c.EnrollmentClientIds.Count / c.Capacity * 100
                    : 0))
            .OrderByDescending(dto => dto.OccupancyRate)
            .ToList();
    }

    public async Task<CoachWorkloadRow> GetCoachWorkloadAsync(
        int coachId,
        DateTime startDate,
        DateTime endDate)
    {
        var coach = await coachRepository.GetByIdAsync(coachId);
        if (coach is null)
            throw new Application.Exceptions.NotFoundException($"Coach with ID {coachId} not found.");

        var classes = await classScheduleRepository.GetClassesByCoachAsync(coachId, startDate, endDate);
        var list = classes.ToList();

        var totalHours = list.Sum(c => (c.EndTimeUtc - c.StartTimeUtc).TotalHours);
        var averageSize = list.Count != 0
            ? (int)list.Average(c => c.EnrollmentClientIds.Count)
            : 0;

        return new CoachWorkloadRow(
            coachId,
            coach.Name.Value,
            list.Count,
            (int)totalHours,
            averageSize);
    }

    public Task<List<CoachEfficiencyRow>> GetCoachEfficiencyAnalyticsAsync(DateTime startDate, DateTime endDate)
        => classScheduleRepository.GetCoachEfficiencyAnalyticsAsync(startDate, endDate);
}
