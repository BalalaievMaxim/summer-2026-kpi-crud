using System.Runtime.CompilerServices;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class ClientAnalyticsRepository(GymManagementContext context) : IClientAnalyticsRepository
{
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
