using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;

namespace GymManagement.Application.Features.Coaches.Queries.GetAllCoaches;

public sealed record GetAllCoachesQuery : IQuery<IReadOnlyList<CoachSummaryDto>>;
