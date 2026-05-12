namespace GymManagement.Application.Features.Clients.ReadModels;

public sealed record ClientDto(
    int ClientId,
    string Name,
    string Email,
    string Phone);

public sealed record ClientSummaryDto(
    int ClientId,
    string Name,
    string Email);
