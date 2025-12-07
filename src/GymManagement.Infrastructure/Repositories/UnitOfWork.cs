using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;

namespace GymManagement.Infrastructure.Repositories;

public class UnitOfWork(GymManagementContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}