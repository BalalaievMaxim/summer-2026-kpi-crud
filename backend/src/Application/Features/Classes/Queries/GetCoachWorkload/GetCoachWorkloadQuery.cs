using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Classes.Queries.GetCoachWorkload;

public sealed record GetCoachWorkloadQuery(int CoachId, DateTime StartDate, DateTime EndDate)
    : IQuery<CoachWorkloadRow>;
