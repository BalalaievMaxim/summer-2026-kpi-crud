using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Application.Services.Interfaces;

public interface IClientService
{
    Task UpdateClientAsync(int clientId, UpdateClientDto updateClientDto);
    Task DeleteClientAsync(int clientId);
    Task<Client> GetClientClassHistoryAsync(int clientId);
    Task<List<Client>> SearchClientsAsync(string searchTerm);
    Task<List<ClientActivityDto>> GetClientActivityAnalyticsAsync();
    Task<Client> RegisterClientAsync(CreateClientDto dto);
    Task<Client> LoginClientAsync(string email, string password);
}