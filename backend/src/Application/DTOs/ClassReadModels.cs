namespace GymManagement.Application.DTOs;

public sealed record GymClassDetails(
    int ClassId,
    int ClassTypeId,
    string ClassTypeName,
    int CoachId,
    string CoachName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int Capacity,
    IReadOnlyList<int> EnrollmentClientIds);

public sealed record ClassTypeDto(
    int ClassTypeId,
    string Name,
    string? Description);
