namespace GymManagement.Domain.Enrollments;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsClientEnrolledAsync(int clientId, int classId, CancellationToken cancellationToken = default);
}
