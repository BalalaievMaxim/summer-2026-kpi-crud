using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Classes.Queries.GetScheduleForWeek;

public sealed record GetScheduleForWeekQuery(DateTime StartOfWeek) : IQuery<IReadOnlyList<GymClassDetails>>;
