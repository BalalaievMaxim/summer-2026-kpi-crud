using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class CoachReadRepository(GymManagementContext context) : ICoachReadRepository
{
    public async Task<IReadOnlyList<CoachSummaryDto>> GetAllSummaryAsync(CancellationToken cancellationToken = default)
    {
        return await context.Coaches
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CoachSummaryDto(
                c.CoachId,
                c.Name,
                c.Specialization
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<CoachDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Coaches
            .AsNoTracking()
            .Where(c => c.CoachId == id)
            .Select(c => new CoachDto(
                c.CoachId,
                c.Name,
                c.Email,
                c.Specialization
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CoachSummaryDto>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default)
    {
        return await context.Coaches
            .AsNoTracking()
            .Where(c => c.Specialization == specialization)
            .OrderBy(c => c.Name)
            .Select(c => new CoachSummaryDto(
                c.CoachId,
                c.Name,
                c.Specialization
            ))
            .ToListAsync(cancellationToken);
    }
}