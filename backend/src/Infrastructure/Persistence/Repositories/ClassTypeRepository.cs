using GymManagement.Application.DTOs;
using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class ClassTypeRepository(GymManagementContext context) : IClassTypeRepositoryPort
{
    public async Task<ClassTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Classtypes
            .AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.ClassTypeId == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await context.Classtypes
            .AsNoTracking()
            .OrderBy(ct => ct.Name)
            .ToListAsync(cancellationToken);

        return list.Select(Map).ToList();
    }

    public async Task<ClassTypeDto> CreateAsync(string name, string? description,
        CancellationToken cancellationToken = default)
    {
        var entity = new E.ClassType
        {
            Name = name,
            Description = description
        };

        context.Classtypes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<ClassTypeDto?> UpdateAsync(int id, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.Classtypes.FindAsync([id], cancellationToken);
        if (existing is null)
            return null;

        existing.Name = name;
        existing.Description = description;

        await context.SaveChangesAsync(cancellationToken);

        return Map(existing);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var classType = await context.Classtypes.FindAsync([id], cancellationToken);
        if (classType is null)
            return false;

        context.Classtypes.Remove(classType);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => context.Classtypes.AnyAsync(ct => ct.ClassTypeId == id, cancellationToken);

    public async Task<ClassTypeDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entity = await context.Classtypes
            .AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.Name == name, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    private static ClassTypeDto Map(E.ClassType ct) =>
        new(ct.ClassTypeId, ct.Name, ct.Description);
}
