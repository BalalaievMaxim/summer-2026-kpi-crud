using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Classes;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Class classEntity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Class classEntity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingClass(int coachId, TimeRange range, int? excludeClassId = null,
        CancellationToken cancellationToken = default);
}
