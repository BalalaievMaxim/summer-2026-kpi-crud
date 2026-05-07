using GymManagement.Application.DTOs;
using GymManagement.Domain.Coaches;

namespace GymManagement.Application.Services.Interfaces;

public interface ICoachService
{
    Task<Coach?> GetByIdAsync(int id);
    Task<IEnumerable<Coach>> GetAllAsync();
    Task<IEnumerable<Coach>> GetBySpecializationAsync(string specialization);
    Task<Coach> RegisterCoachAsync(CreateCoachDto dto);
    Task DeleteCoachAsync(int id);
    Task UpdateSpecializationAsync(int id, string specialization);
}
