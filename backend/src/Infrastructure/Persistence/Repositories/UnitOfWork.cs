using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Persistence;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class UnitOfWork(GymManagementContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}