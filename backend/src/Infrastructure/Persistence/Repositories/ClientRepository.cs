using GymManagement.Domain.Clients;
using GymManagement.Domain.Ports;
using GymManagement.Domain.Queries;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class ClientRepository(GymManagementContext context) : IClientRepository, IClientAnalyticsRepository
{
    
    private static Client ToDomain(Entities.Client e)
        => Client.Reconstitute(e.ClientId, e.Name, e.Email, e.Phone, e.Password);


    public Task<bool> HasActiveEnrollmentsAsync(int clientId, CancellationToken ct = default)
        => context.Enrollments.AnyAsync(e => e.ClientId == clientId, ct);

    public Task<bool> HasActiveMembershipsAsync(int clientId, CancellationToken ct = default)
        => context.Memberships.AnyAsync(m => m.ClientId == clientId && m.IsActive, ct);

    public async Task<Client> AddAsync(Client client, CancellationToken ct = default)
    {
        var entity = new Entities.Client
        {
            Name = client.Name.Value,
            Email = client.Email.Value,
            Phone = client.Phone.Value,
            Password = client.Password.Value,
            CreatedAt = DateTime.UtcNow
        };
        await context.Clients.AddAsync(entity, ct);
        return ToDomain(entity);
    }

    public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var e = await context.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClientId == id, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var lower = email.ToLowerInvariant();
        var e = await context.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == lower, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await context.Clients.AsNoTracking().ToListAsync(ct);
        return list.Select(ToDomain);
    }

    public async Task<IEnumerable<Client>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        var lower = searchTerm.ToLowerInvariant();
        var list = await context.Clients.AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(lower) || c.Email.Contains(lower))
            .ToListAsync(ct);
        return list.Select(ToDomain);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => context.Clients.AnyAsync(c => c.ClientId == id, ct);

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var lower = email.ToLowerInvariant();
        var query = context.Clients.Where(c => c.Email == lower);
        if (excludeId.HasValue)
            query = query.Where(c => c.ClientId != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task UpdateAsync(Client client, CancellationToken ct = default)
    {
        var entity = await context.Clients
            .FirstOrDefaultAsync(c => c.ClientId == client.Id, ct);
        if (entity is null) return;

        entity.Name = client.Name.Value;
        entity.Email = client.Email.Value;
        entity.Phone = client.Phone.Value;
        entity.Password = client.Password.Value;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await context.Clients.FirstOrDefaultAsync(c => c.ClientId == id, ct);
        if (entity is not null)
            context.Clients.Remove(entity);
    }

    public async Task<List<ClientActivityRow>> GetClientActivityAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
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
                ses.client_id AS "ClientId",
                ses.name AS "Name",
                ses.email AS "Email",
                ses.TotalEnrollments AS "TotalEnrollments",
                RANK() OVER (ORDER BY ses.TotalEnrollments DESC) AS "ClientRank"
            FROM ClientEnrollmentStats ses
            ORDER BY "ClientRank" ASC, "TotalEnrollments" DESC;
            """;

        return await context.Database
            .SqlQuery<ClientActivityRow>(FormattableStringFactory.Create(sql))
            .ToListAsync(cancellationToken);
    }
}
