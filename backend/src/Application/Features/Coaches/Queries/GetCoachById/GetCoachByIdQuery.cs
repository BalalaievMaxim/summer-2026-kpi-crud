using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;

namespace GymManagement.Application.Features.Coaches.Queries.GetCoachById;

public sealed record GetCoachByIdQuery(int CoachId) : IQuery<CoachDto?>;
