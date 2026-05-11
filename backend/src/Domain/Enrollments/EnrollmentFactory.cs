using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Domain.Enrollments;

public class EnrollmentFactory
{
    private readonly IClassScheduleRepository _classRepo;
    private readonly IEnrollmentRepositoryPort _enrollmentRepo;

    public EnrollmentFactory(IClassScheduleRepository classRepo, IEnrollmentRepositoryPort enrollmentRepo)
    {
        _classRepo = classRepo;
        _enrollmentRepo = enrollmentRepo;
    }

    public async Task<Enrollment> CreateAsync(
        int clientId,
        int classId,
        CancellationToken cancellationToken = default)
    {
        if (clientId <= 0)
            throw new InvalidEnrollmentError("ClientId must be a positive number.");
        if (classId <= 0)
            throw new InvalidEnrollmentError("ClassId must be a positive number.");

        var classEntity = await _classRepo.GetByIdWithEnrollmentsAsync(classId, cancellationToken);
        if (classEntity is null)
            throw new ClassNotFoundError(classId);

        if (classEntity.EnrollmentClientIds.Count >= classEntity.Capacity)
            throw new ClassFullError(classId);

        var alreadyEnrolled = await _enrollmentRepo.IsClientEnrolledAsync(clientId, classId, cancellationToken);
        if (alreadyEnrolled)
            throw new ClientAlreadyEnrolledError(clientId, classId);

        return Enrollment.Create(clientId, classId);
    }
}
