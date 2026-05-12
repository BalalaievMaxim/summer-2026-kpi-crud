using GymManagement.Application.Features.Coaches.ReadModels;

namespace GymManagement.Application.Services.Interfaces;

public interface ICoachReadRepository
{
    Task<IReadOnlyList<CoachSummaryDto>> GetAllSummaryAsync(CancellationToken cancellationToken = default);
}