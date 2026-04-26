using GymManagement.Repositories.Interfaces;
using GymManagement.Configuration;

namespace GymManagement.Repositories;

public class UnitOfWork(GymManagementContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}