using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class ClientReadRepository(GymManagementContext context) : IClientReadRepository
{
    public async Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Clients
            .AsNoTracking()
            .Where(c => c.ClientId == id)
            .Select(c => new ClientDto(c.ClientId, c.Name, c.Email, c.Phone))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ClientSummaryDto>> SearchAsync(string searchTerm, CancellationToken ct)
    {
        var lower = searchTerm.ToLower();
        return await context.Clients
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(lower) || c.Email.ToLower().Contains(lower))
            .Select(c => new ClientSummaryDto(c.ClientId, c.Name, c.Email))
            .ToListAsync(ct);
    }
}