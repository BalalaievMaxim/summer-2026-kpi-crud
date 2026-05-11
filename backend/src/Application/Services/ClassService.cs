using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Queries;

namespace GymManagement.Application.Services;

public sealed class ClassService(
    IClassScheduleRepository classRepository,
    ICoachRepository coachRepository,
    IClassTypeRepositoryPort classTypeRepository,
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

        var coach = await coachRepository.GetByIdAsync(coachId)
            ?? throw new Application.Exceptions.NotFoundException($"Coach with ID {coachId} not found.");

        if (startTime >= endTime)
            throw new InvalidOperationException("Start time must be before end time.");

        if (startTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot schedule a class in the past.");

        var hasConflict = await classRepository.HasTimeConflictForCoachAsync(
            coachId, startTime, endTime);

        if (hasConflict)
            throw new InvalidOperationException(
                $"Coach {coach.Name.Value} already has a class scheduled during this time.");

        return await classRepository.CreateAsync(classTypeId, coachId, startTime, endTime, capacity);
    }

    public async Task<GymClassDetails?> UpdateClassAsync(int classId, DateTime newStartTime, DateTime newEndTime)
    {
        var classEntity = await classRepository.GetByIdAsync(classId);
        if (classEntity is null)
            return null;

        if (classEntity.StartTimeUtc < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot update a class that has already started.");

        if (newStartTime >= newEndTime)
            throw new InvalidOperationException("Start time must be before end time.");

        if (newStartTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot schedule a class in the past.");

        var hasConflict = await classRepository.HasTimeConflictForCoachAsync(
            classEntity.CoachId,
            newStartTime,
            newEndTime,
            classId);

        if (hasConflict)
            throw new InvalidOperationException(
                "Coach already has a class scheduled during this time.");

        return await classRepository.UpdateTimesAsync(classId, newStartTime, newEndTime);
    }

    public async Task<bool> DeleteClassAsync(int classId)
    {
        var classEntity = await classRepository.GetByIdAsync(classId);
        if (classEntity is null)
            return false;

        if (classEntity.EnrollmentClientIds.Count > 0)
            throw new InvalidOperationException("Cannot delete a class with enrolled clients.");

        await classRepository.DeleteAsync(classId);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task<GymClassDetails?> GetClassByIdAsync(int id)
        => classRepository.GetByIdAsync(id);

    public Task<IReadOnlyList<GymClassDetails>> GetScheduleForDateAsync(DateTime date)
        => classRepository.GetScheduleForDateAsync(date);

    public async Task<IReadOnlyList<GymClassDetails>> GetScheduleForWeekAsync(DateTime startOfWeek)
    {
        var endOfWeek = startOfWeek.AddDays(7);
        return await classRepository.GetScheduleForDateRangeAsync(startOfWeek, endOfWeek);
    }

    public async Task<IReadOnlyList<ClassAttendanceRow>> GetClassAttendanceAnalyticsAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var classes = await classRepository.GetScheduleForDateRangeAsync(startDate, endDate);

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

        var classes = await classRepository.GetClassesByCoachAsync(coachId, startDate, endDate);
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
        => classRepository.GetCoachEfficiencyAnalyticsAsync(startDate, endDate);
}
