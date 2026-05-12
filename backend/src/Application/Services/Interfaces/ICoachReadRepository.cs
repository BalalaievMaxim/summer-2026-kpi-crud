using GymManagement.Application.Features.Coaches.ReadModels;

namespace GymManagement.Application.Services.Interfaces;

public interface ICoachReadRepository
{
    Task<IReadOnlyList<CoachSummaryDto>> GetAllSummaryAsync(CancellationToken cancellationToken = default);
    Task<CoachDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CoachSummaryDto>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default);
}