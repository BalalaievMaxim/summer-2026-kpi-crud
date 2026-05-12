using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Domain.Enrollments;

public class EnrollmentFactory
{
    private readonly IClassRepositoryPort _classRepo;

    public EnrollmentFactory(IClassRepositoryPort classRepo)
    {
        _classRepo = classRepo;
    }

    public async Task<Enrollment> CreateAsync(
        int clientId,
        int classId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        if (clientId <= 0)
            throw new InvalidEnrollmentError("ClientId must be a positive number.");
        if (classId <= 0)
            throw new InvalidEnrollmentError("ClassId must be a positive number.");

        var classEntity = await _classRepo.GetByIdAsync(classId, cancellationToken);
        if (classEntity is null)
            throw new ClassNotFoundError(classId);

        return classEntity.Enroll(clientId, now);
    }
}
