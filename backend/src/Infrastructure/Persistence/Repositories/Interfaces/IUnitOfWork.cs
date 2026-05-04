namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
