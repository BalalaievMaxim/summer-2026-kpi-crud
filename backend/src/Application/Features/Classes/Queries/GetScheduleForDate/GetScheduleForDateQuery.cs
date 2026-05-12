using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Classes.Queries.GetScheduleForDate;

public sealed record GetScheduleForDateQuery(DateTime Date) : IQuery<IReadOnlyList<GymClassDetails>>;
