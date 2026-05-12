using GymManagement.Domain.Classes;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Ports;

public interface IClassRepositoryPort
{
    Task<Class?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Class classEntity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Class classEntity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingClassAsync(
        int coachId,
        TimeRange range,
        int? excludeClassId = null,
        CancellationToken cancellationToken = default);
}
