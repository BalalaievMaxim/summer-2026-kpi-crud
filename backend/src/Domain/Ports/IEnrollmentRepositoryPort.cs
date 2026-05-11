namespace GymManagement.Domain.Ports;

public interface IEnrollmentRepositoryPort
{
    Task<int> AddAsync(int clientId, int classId, DateTime registrationTimeUtc, CancellationToken cancellationToken = default);
    Task<bool> IsClientEnrolledAsync(int clientId, int classId, CancellationToken cancellationToken = default);
}
