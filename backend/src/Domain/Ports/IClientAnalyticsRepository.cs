using GymManagement.Domain.Queries;

namespace GymManagement.Domain.Ports;

public interface IClientAnalyticsRepository
{
    Task<List<ClientActivityRow>> GetClientActivityAnalyticsAsync(CancellationToken cancellationToken = default);
}
