using GymManagement.Application.Exceptions;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Coaches;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

namespace GymManagement.Application.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly ICoachRepository _coachRepository;
    private readonly IClassTypeRepository _classTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClassService(
        IClassRepository classRepository,
        ICoachRepository coachRepository,
        IClassTypeRepository classTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _classRepository = classRepository;
        _coachRepository = coachRepository;
        _classTypeRepository = classTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Class> CreateClassAsync(
        int classTypeId, 
        int coachId, 
        DateTime startTime, 
        DateTime endTime, 
        int capacity)
    {
        var classType = await _classTypeRepository.GetByIdAsync(classTypeId);
        if (classType == null)
            throw new NotFoundException($"ClassType with ID {classTypeId} not found.");

        var coach = await _coachRepository.GetByIdAsync(coachId)
            ?? throw new NotFoundException($"Coach with ID {coachId} not found.");

        if (startTime >= endTime)
            throw new InvalidOperationException("Start time must be before end time.");

        if (startTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot schedule a class in the past.");

        var hasConflict = await _classRepository.HasTimeConflictForCoachAsync(
            coachId, startTime, endTime);
        
        if (hasConflict)
            throw new InvalidOperationException(
                $"Coach {coach.Name} already has a class scheduled during this time.");

        var newClass = new Class
        {
            ClassTypeId = classTypeId,
            CoachId = coachId,
            StartTime = startTime,
            EndTime = endTime,
            Capacity = capacity
        };

        await _classRepository.CreateAsync(newClass);
        await _unitOfWork.SaveChangesAsync();

        return newClass;
    }

    public async Task<Class?> UpdateClassAsync(int classId, DateTime newStartTime, DateTime newEndTime)
    {
        var classEntity = await _classRepository.GetByIdAsync(classId);
        if (classEntity == null)
            return null;

        if (classEntity.StartTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot update a class that has already started.");

        if (newStartTime >= newEndTime)
            throw new InvalidOperationException("Start time must be before end time.");

        if (newStartTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot schedule a class in the past.");

        var hasConflict = await _classRepository.HasTimeConflictForCoachAsync(
            classEntity.CoachId, 
            newStartTime, 
            newEndTime, 
            classId);
        
        if (hasConflict)
            throw new InvalidOperationException(
                "Coach already has a class scheduled during this time.");

        classEntity.StartTime = newStartTime;
        classEntity.EndTime = newEndTime;

        await _classRepository.UpdateAsync(classEntity);
        await _unitOfWork.SaveChangesAsync();

        return classEntity;
    }

    public async Task<bool> DeleteClassAsync(int classId)
    {
        var classEntity = await _classRepository.GetByIdAsync(classId);
        if (classEntity == null)
            return false;

        if (classEntity.Enrollments.Count > 0)
            throw new InvalidOperationException("Cannot delete a class with enrolled clients.");

        await _classRepository.DeleteAsync(classId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Class>> GetScheduleForDateAsync(DateTime date)
    {
        return await _classRepository.GetScheduleForDateAsync(date);
    }

    public async Task<IEnumerable<Class>> GetScheduleForWeekAsync(DateTime startOfWeek)
    {
        var endOfWeek = startOfWeek.AddDays(7);
        return await _classRepository.GetScheduleForDateRangeAsync(startOfWeek, endOfWeek);
    }

    public async Task<IEnumerable<ClassAttendanceDto>> GetClassAttendanceAnalyticsAsync(
        DateTime startDate, 
        DateTime endDate)
    {
        var classes = await _classRepository.GetScheduleForDateRangeAsync(startDate, endDate);

        return classes.Select(c => new ClassAttendanceDto
        {
            ClassId = c.ClassId,
            ClassName = c.ClassType.Name,
            CoachName = c.Coach.Name,
            StartTime = c.StartTime,
            Capacity = c.Capacity,
            CurrentEnrollment = c.Enrollments.Count,
            OccupancyRate = c.Capacity > 0 
                ? (decimal)c.Enrollments.Count / c.Capacity * 100 
                : 0
        }).OrderByDescending(dto => dto.OccupancyRate);
    }

    public async Task<CoachWorkloadDto> GetCoachWorkloadAsync(
        int coachId, 
        DateTime startDate, 
        DateTime endDate)
    {
        var coach = await _coachRepository.GetByIdAsync(coachId);
        if (coach == null)
            throw new NotFoundException($"Coach with ID {coachId} not found.");

        var classes = await _classRepository.GetClassesByCoachAsync(coachId, startDate, endDate);

        var totalHours = classes.Sum(c => (c.EndTime - c.StartTime).TotalHours);
        var averageSize = classes.Any() 
            ? (int)classes.Average(c => c.Enrollments.Count)
            : 0;

        return new CoachWorkloadDto
        {
            CoachId = coachId,
            CoachName = coach.Name.Value,
            TotalClassesScheduled = classes.Count(),
            TotalHoursWorked = (int)totalHours,
            AverageClassSize = averageSize
        };
    }
}