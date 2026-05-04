using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Infrastructure.Persistence;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class UnitOfWork(GymManagementContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}