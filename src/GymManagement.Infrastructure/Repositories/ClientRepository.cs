using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Repositories;

public class ClientRepository (GymManagementContext context): IClientRepository
{
    public async Task<Client?> GetClientByIdAsync(int clientId)
    {
        return await context.Clients
            .Where(c => c.ClientId == clientId)
            .FirstOrDefaultAsync();
    }
    
    public async Task AddAsync(Client client)
    {
        await context.Clients.AddAsync(client);
    }
}