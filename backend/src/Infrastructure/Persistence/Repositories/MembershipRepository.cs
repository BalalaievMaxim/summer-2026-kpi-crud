using GymManagement.Domain.Memberships;
using GymManagement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly GymManagementContext _context;

    public MembershipRepository(GymManagementContext context)
    {
        _context = context;
    }

    public async Task<Membership?> GetByIdAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Memberships
            .Include(m => m.MembershipPlan)
            .ThenInclude(p => p.PlanAccesses)
            .FirstOrDefaultAsync(m => m.MembershipId == intId);

        return entity is null ? null : MembershipMapper.ToDomain(entity);
    }

    public async Task<List<Membership>> GetActiveByClientAsync(Guid clientId)
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

    public async Task AddAsync(Membership membership)
    {
        var entity = MembershipMapper.ToEntity(membership);
        await _context.Memberships.AddAsync(entity);
    }

    public async Task UpdateAsync(Membership membership)
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
