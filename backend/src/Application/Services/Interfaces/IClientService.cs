using GymManagement.Application.DTOs;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Queries;

namespace GymManagement.Application.Services.Interfaces;

public interface IClientService
{
    Task<Client> RegisterClientAsync(CreateClientDto dto);
    Task<Client> LoginClientAsync(string email, string password);
    Task UpdateClientAsync(int clientId, UpdateClientDto dto);
    Task DeleteClientAsync(int clientId);
    Task<Client?> GetByIdAsync(int clientId);
    Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm);
    Task<List<ClientActivityRow>> GetClientActivityAnalyticsAsync();
}
