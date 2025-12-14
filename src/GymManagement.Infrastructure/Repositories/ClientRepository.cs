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

    public async Task<Client?> GetByIdWithMembershipsAsync(int clientId)
    {
        return await context.Clients
            .Include(c => c.Memberships)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
    }



    public async Task<Client?> GetByIdWithEnrollmentsAsync(int clientId)
    {
        return await context.Clients
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
    }

    public Task<List<Client>> SearchByNameOrEmailAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null)
    {
        var query = context.Clients.Where(c => c.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.ClientId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public Task UpdateAsync(Client client)
    {
        context.Clients.Update(client);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Client client)
    {
        context.Clients.Remove(client);
        return Task.CompletedTask;
    }

    public Task<List<Client>> ListAsync()
    {
        throw new NotImplementedException();
    }
}