using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IClientService
{
    Task UpdateClientAsync(int clientId, UpdateClientDto updateClientDto);
    Task DeleteClientAsync(int clientId);
    Task<Client> GetClientClassHistoryAsync(int clientId);
    Task<List<Client>> SearchClientsAsync(string searchTerm);
    Task<List<ClientActivityDto>> GetClientActivityAnalyticsAsync();
}