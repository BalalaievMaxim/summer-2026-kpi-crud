using GymManagement.Infrastructure.DTOs;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IClientAnalyticsRepository
{
    Task<List<ClientActivityDto>> GetClientActivityAnalyticsAsync();
}
