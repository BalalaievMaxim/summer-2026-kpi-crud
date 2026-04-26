using GymManagement.Models;
using GymManagement.Repositories.Interfaces;
using GymManagement.Configuration;
using Microsoft.EntityFrameworkCore;      

namespace GymManagement.Repositories;

public class ClassTypeRepository : IClassTypeRepository
{
    private readonly GymManagementContext _context;

    public ClassTypeRepository(GymManagementContext context)
    {
        _context = context;
    }

    public async Task<ClassType?> GetByIdAsync(int id)
    {
        return await _context.Classtypes
            .Include(ct => ct.Classes)
            .FirstOrDefaultAsync(ct => ct.ClassTypeId == id);
    }

    public async Task<IEnumerable<ClassType>> GetAllAsync()
    {
        return await _context.Classtypes
            .OrderBy(ct => ct.Name)
            .ToListAsync();
    }

    public async Task<ClassType> CreateAsync(ClassType classType)
    {
        _context.Classtypes.Add(classType);
        await _context.SaveChangesAsync();
        return classType;
    }

    public async Task<ClassType?> UpdateAsync(ClassType classType)
    {
        var existing = await _context.Classtypes.FindAsync(classType.ClassTypeId);
        if (existing == null)
        {
            return null;
        }

        existing.Name = classType.Name;
        existing.Description = classType.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var classType = await _context.Classtypes.FindAsync(id);
        if (classType == null)
        {
            return false;
        }

        _context.Classtypes.Remove(classType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Classtypes.AnyAsync(ct => ct.ClassTypeId == id);
    }

    public async Task<ClassType?> GetByNameAsync(string name)
    {
        return await _context.Classtypes
            .FirstOrDefaultAsync(ct => ct.Name == name);
    }
}
