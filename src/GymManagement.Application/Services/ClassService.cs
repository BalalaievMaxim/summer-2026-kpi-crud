using GymManagement.Core.Entities;
using GymManagement.Core.Exceptions;
using GymManagement.Core.Interfaces;
using GymManagement.Core.DTOs;

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

        var coach = await _coachRepository.GetByIdAsync(coachId);
        if (coach == null)
            throw new NotFoundException($"Coach with ID {coachId} not found.");

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
            Capacity = capacity,
            CurrentEnrollment = 0
        };

        await _classRepository.CreateAsync(newClass);
        await _unitOfWork.SaveChangesAsync();

        return newClass;
    }

    public async Task<bool> DeleteClassAsync(int classId)
    {
        var classEntity = await _classRepository.GetByIdAsync(classId);
        if (classEntity == null)
            return false;

        if (classEntity.CurrentEnrollment > 0)
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
            CurrentEnrollment = c.CurrentEnrollment,
            OccupancyRate = c.Capacity > 0 
                ? (decimal)c.CurrentEnrollment / c.Capacity * 100 
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
            ? (int)classes.Average(c => c.CurrentEnrollment) 
            : 0;

        return new CoachWorkloadDto
        {
            CoachId = coachId,
            CoachName = coach.Name,
            TotalClassesScheduled = classes.Count(),
            TotalHoursWorked = (int)totalHours,
            AverageClassSize = averageSize
        };
    }
}
