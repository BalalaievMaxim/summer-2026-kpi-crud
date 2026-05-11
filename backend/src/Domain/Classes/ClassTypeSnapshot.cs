namespace GymManagement.Domain.Classes;

public sealed record ClassTypeSnapshot(
    int ClassTypeId,
    string Name,
    string? Description);
