namespace GymManagement.Domain.Coaches;

public interface ICoachRepository
{
    Task<Coach?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Coach>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Coach>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> HasUpcomingClassesWithEnrollmentsAsync(int coachId, CancellationToken cancellationToken = default);

    Task<Coach> AddAsync(Coach coach, CancellationToken cancellationToken = default);
    Task UpdateAsync(Coach coach, CancellationToken cancellationToken = default);
    Task DeleteUpcomingClassesByCoachAsync(int coachId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
