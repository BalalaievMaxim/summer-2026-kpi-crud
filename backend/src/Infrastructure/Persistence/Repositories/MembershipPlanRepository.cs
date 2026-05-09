using DomainMembershipPlan = GymManagement.Domain.Memberships.MembershipPlan;
using DomainIMembershipPlanRepository = GymManagement.Domain.Memberships.IMembershipPlanRepository;
using EfMembershipPlan = GymManagement.Infrastructure.Persistence.Entities.MembershipPlan;
using OldIMembershipPlanRepository = GymManagement.Infrastructure.Persistence.Repositories.Interfaces.IMembershipPlanRepository;
using GymManagement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipPlanRepository : OldIMembershipPlanRepository, DomainIMembershipPlanRepository
{
    private readonly GymManagementContext _context;

    public MembershipPlanRepository(GymManagementContext context)
    {
        _context = context;
    }

    // ── старий інтерфейс ──
    public async Task AddAsync(EfMembershipPlan membershipPlan)
    {
        await _context.Membershipplans.AddAsync(membershipPlan);
    }

    public async Task<EfMembershipPlan?> GetMembershipPlanByIdAsync(int planId)
    {
        return await _context.Membershipplans
            .Include(p => p.PlanAccesses)
            .FirstOrDefaultAsync(p => p.PlanId == planId);
    }

    public async Task DeleteMembershipPlanAsync(int planId)
    {
        var entity = await _context.Membershipplans.FindAsync(planId);
        if (entity is not null)
            _context.Membershipplans.Remove(entity);
    }

    public async Task<List<EfMembershipPlan>> GetPlansAsync(decimal? min, decimal? max)
    {
        var query = _context.Membershipplans.AsQueryable();
        if (min.HasValue) query = query.Where(p => p.Price >= min.Value);
        if (max.HasValue) query = query.Where(p => p.Price <= max.Value);
        return await query.ToListAsync();
    }

    // ── новий доменний інтерфейс ──
    public async Task<DomainMembershipPlan?> GetByIdAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Membershipplans
            .Include(p => p.PlanAccesses)
            .FirstOrDefaultAsync(p => p.PlanId == intId);
        return entity is null ? null : MembershipPlanMapper.ToDomain(entity);
    }

    public async Task<List<DomainMembershipPlan>> GetAllAsync()
    {
        var entities = await _context.Membershipplans
            .Include(p => p.PlanAccesses)
            .ToListAsync();
        return entities.Select(MembershipPlanMapper.ToDomain).ToList();
    }

    public async Task AddAsync(DomainMembershipPlan plan)
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
