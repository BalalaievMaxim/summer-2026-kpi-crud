using GymManagement.Domain.Memberships;
using GymManagement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipPlanRepository : IMembershipPlanRepository
{
    private readonly GymManagementContext _context;

    public MembershipPlanRepository(GymManagementContext context)
    {
        _context = context;
    }

    public async Task<MembershipPlan?> GetByIdAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Membershipplans
            .Include(p => p.PlanAccesses)
            .FirstOrDefaultAsync(p => p.PlanId == intId);

        return entity is null ? null : MembershipPlanMapper.ToDomain(entity);
    }

    public async Task<List<MembershipPlan>> GetAllAsync()
    {
        var entities = await _context.Membershipplans
            .Include(p => p.PlanAccesses)
            .ToListAsync();

        return entities.Select(MembershipPlanMapper.ToDomain).ToList();
    }

    public async Task AddAsync(MembershipPlan plan)
    {
        var entity = MembershipPlanMapper.ToEntity(plan);
        await _context.Membershipplans.AddAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Membershipplans.FindAsync(intId);
        if (entity is not null)
            _context.Membershipplans.Remove(entity);
    }

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
