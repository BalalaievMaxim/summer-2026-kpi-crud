using GymManagement.Application.Features.Clients.ReadModels;

namespace GymManagement.Application.Services.Interfaces;

public interface IClientReadRepository
{
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ClientSummaryDto>> SearchAsync(string searchTerm, CancellationToken ct);
}