namespace GymManagement.Domain.Classes;

public interface IClassTypeRepository
{
    Task<ClassType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClassType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ClassType classType, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
