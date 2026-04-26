using GymManagement.Models;

namespace GymManagement.Repositories.Interfaces;

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
