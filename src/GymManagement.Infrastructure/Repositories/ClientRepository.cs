using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Core.DTOs;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Repositories;

public class ClientRepository(GymManagementContext context) : IClientRepository
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

    public async Task<List<Client>> SearchByNameOrEmailAsync(string searchTerm)
    {
        return await context.Clients
            .Where(c => c.Name.Contains(searchTerm) || c.Email.Contains(searchTerm))
            .ToListAsync();
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

    public async Task<List<Client>> ListAsync()
    {
        return await context.Clients.ToListAsync();
    }

    // складний аналітичний запит для отримання активності клієнтів
    // підрахунок кількості записів на заняття за останній місяць та обрахунок рангу
    public async Task<List<ClientActivityDto>> GetClientActivityAnalyticsAsync()
    {
        var sql = @"
            WITH ClientEnrollmentStats AS (
                SELECT
                    c.client_id,
                    c.name,
                    c.email,
                    COUNT(e.enrollment_id) AS TotalEnrollments
                FROM client c
                LEFT JOIN enrollment e ON c.client_id = e.client_id
                WHERE e.registration_time >= NOW() - INTERVAL '1 month'
                GROUP BY c.client_id, c.name, c.email
            )
            SELECT
                ses.client_id AS ""ClientId"",
                ses.name AS ""Name"",
                ses.email AS ""Email"",
                ses.TotalEnrollments AS ""TotalEnrollments"",
                -- Віконна функція RANK() OVER для ранжування за кількістю записів
                RANK() OVER (ORDER BY ses.TotalEnrollments DESC) AS ""ClientRank""
            FROM ClientEnrollmentStats ses
            ORDER BY ""ClientRank"" ASC, ""TotalEnrollments"" DESC;
        ";
        
        return await context.Database
            .SqlQuery<ClientActivityDto>(System.Runtime.CompilerServices.FormattableStringFactory.Create(sql))
            .ToListAsync();
    }
}
