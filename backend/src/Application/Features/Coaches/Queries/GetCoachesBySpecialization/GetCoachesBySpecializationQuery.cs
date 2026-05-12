using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Coaches.ReadModels;

namespace GymManagement.Application.Features.Coaches.Queries.GetCoachesBySpecialization;

public sealed record GetCoachesBySpecializationQuery(string Specialization) : IQuery<IReadOnlyList<CoachSummaryDto>>;
