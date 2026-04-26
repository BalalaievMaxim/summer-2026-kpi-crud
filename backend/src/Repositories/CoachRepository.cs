using GymManagement.Models;
using GymManagement.Repositories.Interfaces;
using GymManagement.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Repositories;

public class CoachRepository : ICoachRepository
{
    private readonly GymManagementContext _context;

    public CoachRepository(GymManagementContext context)
    {
        _context = context;
    }

    public async Task<Coach?> GetByIdAsync(int id)
    {
        return await _context.Coaches
            .Include(c => c.Classes)
                .ThenInclude(cl => cl.ClassType)
            .FirstOrDefaultAsync(c => c.CoachId == id);
    }

    public async Task<IEnumerable<Coach>> GetAllAsync()
    {
        return await _context.Coaches
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Coach> CreateAsync(Coach coach)
    {
        if (await EmailExistsAsync(coach.Email))
        {
            throw new InvalidOperationException($"Coach with email {coach.Email} already exists");
        }

        _context.Coaches.Add(coach);
        await _context.SaveChangesAsync();
        return coach;
    }

    public async Task<Coach?> UpdateAsync(Coach coach)
    {
        var existing = await _context.Coaches.FindAsync(coach.CoachId);
        if (existing == null)
        {
            return null;
        }

        if (existing.Email != coach.Email && await EmailExistsAsync(coach.Email))
        {
            throw new InvalidOperationException($"Email {coach.Email} is already taken");
        }

        existing.Name = coach.Name;
        existing.Specialization = coach.Specialization;
        existing.Email = coach.Email;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var coach = await _context.Coaches.FindAsync(id);
        if (coach == null)
        {
            return false;
        }

        _context.Coaches.Remove(coach);
        return true;
    }

    public async Task<IEnumerable<Coach>> GetBySpecializationAsync(string specialization)
    {
        return await _context.Coaches
            .Where(c => c.Specialization == specialization)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Coaches.AnyAsync(c => c.Email == email);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Coaches.AnyAsync(c => c.CoachId == id);
    }

    public async Task<bool> HasScheduledClassesAsync(int coachId, DateTime from, DateTime to)
    {
        return await _context.Classes
            .AnyAsync(c => c.CoachId == coachId 
                && c.StartTime >= from 
                && c.EndTime <= to);
    }
}
