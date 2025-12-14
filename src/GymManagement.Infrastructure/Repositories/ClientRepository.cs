using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Repositories;

public class ClientRepository (GymManagementContext context): IClientRepository
{
    public Task<Client?> GetClientByIdAsync(int clientId)
    {
        return GetByIdAsync(clientId);
    }

    public async Task<Client?> GetByIdAsync(int clientId)
    {
        return await context.Clients
            .Where(c => c.ClientId == clientId)
            .FirstOrDefaultAsync();
    }
    
    public async Task AddAsync(Client client)
    {
        await context.Clients.AddAsync(client);
    }

    public Task<Client?> GetByIdWithEnrollmentsAsync(int clientId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Client>> SearchByNameOrEmailAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Client client)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(Client client)
    {
        throw new NotImplementedException();
    }

    public Task<List<Client>> ListAsync()
    {
        throw new NotImplementedException();
    }
}