using DomainMembership = GymManagement.Domain.Memberships.Membership;
using DomainIMembershipRepository = GymManagement.Domain.Memberships.IMembershipRepository;
using EfMembership = GymManagement.Infrastructure.Persistence.Entities.Membership;
using OldIMembershipRepository = GymManagement.Infrastructure.Persistence.Repositories.Interfaces.IMembershipRepository;
using GymManagement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipRepository : OldIMembershipRepository, DomainIMembershipRepository
{
    private readonly GymManagementContext _context;

    public MembershipRepository(GymManagementContext context)
    {
        _context = context;
    }

    // ── старий інтерфейс ──
    public async Task AddAsync(EfMembership membership)
    {
        await _context.Memberships.AddAsync(membership);
    }

    public async Task<List<EfMembership>> GetActiveMembershipsByClientAsync(int clientId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == true && m.EndDate > today)
            .ToListAsync();
    }

    public async Task MarkAsActiveMembershipAsync(int membershipId)
    {
        await _context.Memberships
            .Where(m => m.MembershipId == membershipId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsActive, true));
    }

    public async Task<List<EfMembership>> GetAllActiveMembershipReferencedOnMembershipPlan(int planId)
    {
        return await _context.Memberships
            .Where(m => m.PlanId == planId && m.IsActive == true)
            .ToListAsync();
    }

    public async Task<EfMembership?> GetPendingMembershipByClientAsync(int clientId)
    {
        return await _context.Memberships
            .Where(m => m.ClientId == clientId && m.IsActive == false)
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefaultAsync();
    }

    // ── новий доменний інтерфейс ──
    public async Task<DomainMembership?> GetByIdAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Memberships
            .Include(m => m.MembershipPlan)
            .ThenInclude(p => p.PlanAccesses)
            .FirstOrDefaultAsync(m => m.MembershipId == intId);
        return entity is null ? null : MembershipMapper.ToDomain(entity);
    }

    public async Task<List<DomainMembership>> GetActiveByClientAsync(Guid clientId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var intClientId = GuidToInt(clientId);
        var entities = await _context.Memberships
            .Where(m => m.ClientId == intClientId && m.IsActive && m.EndDate >= today)
            .ToListAsync();
        return entities.Select(MembershipMapper.ToDomain).ToList();
    }

    public async Task<bool> HasActiveMembershipAsync(Guid clientId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var intClientId = GuidToInt(clientId);
        return await _context.Memberships
            .AnyAsync(m => m.ClientId == intClientId && m.IsActive && m.EndDate >= today);
    }

    public async Task AddAsync(DomainMembership membership)
    {
        var entity = MembershipMapper.ToEntity(membership);
        await _context.Memberships.AddAsync(entity);
    }

    public async Task UpdateAsync(DomainMembership membership)
    {
        var entity = MembershipMapper.ToEntity(membership);
        _context.Memberships.Update(entity);
        await Task.CompletedTask;
    }

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
