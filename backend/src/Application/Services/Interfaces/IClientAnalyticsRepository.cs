using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IClientAnalyticsRepository
{
    Task<List<ClientActivityRow>> GetClientActivityAnalyticsAsync(CancellationToken cancellationToken = default);
}
