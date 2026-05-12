namespace GymManagement.Domain.Ports;

using GymManagement.Domain.Enrollments;

public interface IEnrollmentRepositoryPort
{
    Task<int> AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task<bool> IsClientEnrolledAsync(int clientId, int classId, CancellationToken cancellationToken = default);
}
