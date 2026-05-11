using GymManagement.Domain.Classes;

namespace GymManagement.Domain.Ports;

public interface IClassTypeRepositoryPort
{
    Task<ClassTypeSnapshot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassTypeSnapshot>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassTypeSnapshot> CreateAsync(string name, string? description, CancellationToken cancellationToken = default);
    Task<ClassTypeSnapshot?> UpdateAsync(int id, string name, string? description, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<ClassTypeSnapshot?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
