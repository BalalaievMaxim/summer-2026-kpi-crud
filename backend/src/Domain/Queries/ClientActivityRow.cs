namespace GymManagement.Domain.Queries;

public sealed record ClientActivityRow(
    int ClientId,
    string Name,
    string Email,
    int TotalEnrollments,
    int ClientRank);
