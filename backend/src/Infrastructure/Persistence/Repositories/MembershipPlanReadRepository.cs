using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class MembershipPlanReadRepository(GymManagementContext context) : IMembershipPlanReadRepository
{
    public async Task<MembershipPlanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Membershipplans
            .AsNoTracking()
            .Where(p => p.PlanId == id)
            .Select(p => new MembershipPlanDto(
                p.PlanId,
                p.Name,
                p.DurationMonths,
                p.Price
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<MembershipPlanDto>> GetPlansAsync(decimal? min, decimal? max, CancellationToken cancellationToken = default)
    {
        var query = context.Membershipplans.AsNoTracking().AsQueryable();
        
        if (min.HasValue) query = query.Where(p => p.Price >= min.Value);
        if (max.HasValue) query = query.Where(p => p.Price <= max.Value);

        return await query
            .Select(p => new MembershipPlanDto(
                p.PlanId,
                p.Name,
                p.DurationMonths,
                p.Price
            ))
            .ToListAsync(cancellationToken);
    }
}