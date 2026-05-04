using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IClassTypeRepository
{
    Task<ClassType?> GetByIdAsync(int id);
    Task<IEnumerable<ClassType>> GetAllAsync();
    Task<ClassType> CreateAsync(ClassType classType);
    Task<ClassType?> UpdateAsync(ClassType classType);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<ClassType?> GetByNameAsync(string name);
}
