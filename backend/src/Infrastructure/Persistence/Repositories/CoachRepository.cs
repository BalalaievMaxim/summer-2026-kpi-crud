using GymManagement.Domain.Coaches;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class CoachRepository(GymManagementContext context) : ICoachRepository
{
    private static Coach ToDomain(Entities.Coach e)
        => Coach.Reconstitute(e.CoachId, e.Name, e.Email, e.Specialization, e.Password);

    public async Task<Coach> AddAsync(Coach coach, CancellationToken ct = default)
    {
        var entity = new Entities.Coach
        {
            Name = coach.Name.Value,
            Email = coach.Email.Value,
            Specialization = coach.Specialization.Value,
            Password = coach.Password.Value,
            CreatedAt = DateTime.UtcNow
        };
        context.Coaches.Add(entity);
        return ToDomain(entity);
    }

    public async Task<Coach?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var e = await context.Coaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CoachId == id, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task<IEnumerable<Coach>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await context.Coaches.AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
        return list.Select(ToDomain);
    }

    public async Task<IEnumerable<Coach>> GetBySpecializationAsync(string specialization, CancellationToken ct = default)
    {
        var list = await context.Coaches.AsNoTracking()
            .Where(c => c.Specialization == specialization)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
        return list.Select(ToDomain);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => context.Coaches.AnyAsync(c => c.CoachId == id, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => context.Coaches.AnyAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> HasUpcomingClassesWithEnrollmentsAsync(int coachId, CancellationToken ct = default)
        => await context.Classes
            .Where(c => c.CoachId == coachId && c.StartTime > DateTime.UtcNow)
            .AnyAsync(c => c.Enrollments.Count > 0, ct);

    public async Task UpdateAsync(Coach coach, CancellationToken ct = default)
    {
        var entity = await context.Coaches
            .FirstOrDefaultAsync(c => c.CoachId == coach.Id, ct);
        if (entity is null) return;

        entity.Name = coach.Name.Value;
        entity.Email = coach.Email.Value;
        entity.Specialization = coach.Specialization.Value;
        entity.Password = coach.Password.Value;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await context.Coaches.FindAsync([id], ct);
        if (entity is null) return false;
        context.Coaches.Remove(entity);
        return true;
    }

    public async Task DeleteUpcomingClassesByCoachAsync(int coachId, CancellationToken ct = default)
    {
        var upcomingClasses = await context.Classes
            .Where(c => c.CoachId == coachId && c.StartTime > DateTime.UtcNow)
            .ToListAsync(ct);

        context.Classes.RemoveRange(upcomingClasses);
    }
}
