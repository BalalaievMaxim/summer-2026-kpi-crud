namespace GymManagement.Domain.Ports;

public interface IClassTypeRepositoryPort
{
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
