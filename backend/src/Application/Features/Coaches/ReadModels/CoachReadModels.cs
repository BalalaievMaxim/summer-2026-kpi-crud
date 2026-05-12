namespace GymManagement.Application.Features.Coaches.ReadModels;

public sealed record CoachDto(
    int CoachId,
    string Name,
    string Email,
    string Specialization);

public sealed record CoachSummaryDto(
    int CoachId,
    string Name,
    string Specialization);
