using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments.Errors;

namespace GymManagement.Domain.Enrollments;

public class EnrollmentFactory
{
    private readonly IClassRepository _classRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;

    public EnrollmentFactory(IClassRepository classRepo, IEnrollmentRepository enrollmentRepo)
    {
        _classRepo = classRepo;
        _enrollmentRepo = enrollmentRepo;
    }

    /// <summary>
    /// Creates a new Enrollment after validating all invariants:
    /// - ClientId and ClassId are positive
    /// - Class exists and is not full
    /// - Client is not already enrolled in the class
    /// </summary>
    public async Task<Enrollment> CreateAsync(
        int clientId,
        int classId,
        CancellationToken cancellationToken = default)
    {
        // Simple invariants
        if (clientId <= 0)
            throw new InvalidEnrollmentError("ClientId must be a positive number.");
        if (classId <= 0)
            throw new InvalidEnrollmentError("ClassId must be a positive number.");

        // Complex invariants (require repository)
        var classEntity = await _classRepo.GetByIdAsync(classId, cancellationToken);
        if (classEntity is null)
            throw new ClassNotFoundError(classId);

        if (classEntity.IsFull)
            throw new ClassFullError(classId);

        var alreadyEnrolled = await _enrollmentRepo.IsClientEnrolledAsync(clientId, classId, cancellationToken);
        if (alreadyEnrolled)
            throw new ClientAlreadyEnrolledError(clientId, classId);

        return Enrollment.Create(clientId, classId);
    }
}
