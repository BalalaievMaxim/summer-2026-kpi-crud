namespace GymManagement.Application.DTOs;

public sealed record AuthResultDto(
    int ClientId,
    string Name,
    string Email,
    string Phone,
    string Token
);