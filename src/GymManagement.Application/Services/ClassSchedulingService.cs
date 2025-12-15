using GymManagement.Core.Entities;
using GymManagement.Core.Exceptions;
using GymManagement.Core.Interfaces;
using GymManagement.Core.DTOs;

namespace GymManagement.Application.Services;

public class ClassSchedulingService : IClassSchedulingService
{
    private readonly IClassRepository _classRepository;
    private readonly ICoachRepository _coachRepository;
    private readonly IClassTypeRepository _classTypeRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClassSchedulingService(
        IClassRepository classRepository,
        ICoachRepository coachRepository,
        IClassTypeRepository classTypeRepository,
        IClientRepository clientRepository,
        IEnrollmentRepository enrollmentRepository,
        IUnitOfWork unitOfWork)
    {
        _classRepository = classRepository;
        _coachRepository = coachRepository;
        _classTypeRepository = classTypeRepository;
        _clientRepository = clientRepository;
        _enrollmentRepository = enrollmentRepository;
        _unitOfWork = unitOfWork;
    }

    // create class with validation
    public async Task<Class> CreateClassAsync(
        int classTypeId, 
        int coachId, 
        DateTime startTime, 
        DateTime endTime, 
        int capacity)
    {
        // 1. check ClassType
        var classType = await _classTypeRepository.GetByIdAsync(classTypeId);
        if (classType == null)
            throw new NotFoundException($"ClassType with ID {classTypeId} not found.");

        // 2. check Coach
        var coach = await _coachRepository.GetByIdAsync(coachId);
        if (coach == null)
            throw new NotFoundException($"Coach with ID {coachId} not found.");

        // time validation
        if (startTime >= endTime)
            throw new InvalidOperationException("Start time must be before end time.");

        if (startTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot schedule a class in the past.");

        // check conflicts coach
        var hasConflict = await _classRepository.HasTimeConflictForCoachAsync(
            coachId, startTime, endTime);
        
        if (hasConflict)
            throw new InvalidOperationException(
                $"Coach {coach.Name} already has a class scheduled during this time.");

        // create class
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

    // write client to enrollment
    public async Task<Enrollment> EnrollClientAsync(int clientId, int classId)
    {
        // check client in db
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
            throw new NotFoundException($"Client with ID {clientId} not found.");

        var classEntity = await _classRepository.GetByIdAsync(classId);
        if (classEntity == null)
            throw new NotFoundException($"Class with ID {classId} not found.");

        // check active membership
        if (client.Membership == null || client.Membership.Status != "active")
            throw new InvalidOperationException("Client does not have an active membership.");

        if (classEntity.StartTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot enroll in a past class.");

        if (classEntity.CurrentEnrollment >= classEntity.Capacity)
            throw new InvalidOperationException("Class is already full.");

        var existingEnrollment = await _enrollmentRepository.GetEnrollmentAsync(clientId, classId);
        if (existingEnrollment != null)
            throw new InvalidOperationException("Client is already enrolled in this class.");

        // create enrollment
        var enrollment = new Enrollment
        {
            ClientId = clientId,
            ClassId = classId,
            RegistrationTime = DateTime.UtcNow
        };

        await _enrollmentRepository.CreateAsync(enrollment);

        // update num CurrentEnrollment
        classEntity.CurrentEnrollment++;
        await _classRepository.UpdateAsync(classEntity);

        await _unitOfWork.SaveChangesAsync(); 

        return enrollment;
    }

    public async Task<bool> CancelEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment == null)
            return false;

        var classEntity = await _classRepository.GetByIdAsync(enrollment.ClassId);
        if (classEntity == null)
            throw new NotFoundException($"Class with ID {enrollment.ClassId} not found.");

        if (classEntity.StartTime < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot cancel enrollment for a class that has already started.");

        await _enrollmentRepository.DeleteAsync(enrollmentId);
        
        classEntity.CurrentEnrollment--;
        await _classRepository.UpdateAsync(classEntity);

        await _unitOfWork.SaveChangesAsync(); 

        return true;
    }

    // schedule
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

    // analitic workplan coach 
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
