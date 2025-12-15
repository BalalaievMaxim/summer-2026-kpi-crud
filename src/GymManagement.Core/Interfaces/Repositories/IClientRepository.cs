using GymManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Core.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetClientByIdAsync(int clientId);
    Task<Client?> GetByIdAsync(int clientId);
    Task<Client?> GetByIdWithMembershipsAsync(int clientId);
    Task<Client?> GetByIdWithEnrollmentsAsync(int clientId);
    Task<List<Client>> SearchByNameOrEmailAsync(string searchTerm);
    Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task RemoveAsync(Client client);
    Task<List<Client>> ListAsync();
}