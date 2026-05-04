using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface ICoachRepository
{
    Task<Coach?> GetByIdAsync(int id);
    Task<IEnumerable<Coach>> GetAllAsync();
    Task<Coach> CreateAsync(Coach coach);
    Task<Coach?> UpdateAsync(Coach coach);
    Task<bool> DeleteAsync(int id);
    
    Task<IEnumerable<Coach>> GetBySpecializationAsync(string specialization);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> ExistsAsync(int id);
  
    Task<bool> HasScheduledClassesAsync(int coachId, DateTime from, DateTime to);
}
