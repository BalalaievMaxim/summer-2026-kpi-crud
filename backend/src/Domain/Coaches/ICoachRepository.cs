namespace GymManagement.Domain.Coaches;

public interface ICoachRepository
{
    Task<Coach?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Coach>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Coach coach, CancellationToken cancellationToken = default);
    Task UpdateAsync(Coach coach, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
