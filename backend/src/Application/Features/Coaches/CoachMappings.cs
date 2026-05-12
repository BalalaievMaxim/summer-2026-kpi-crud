using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Domain.Coaches;

namespace GymManagement.Application.Features.Coaches;

internal static class CoachMappings
{
    public static CoachDto ToDto(Coach coach)
        => new(coach.Id, coach.Name.Value, coach.Email.Value, coach.Specialization.Value);

    public static CoachSummaryDto ToSummaryDto(Coach coach)
        => new(coach.Id, coach.Name.Value, coach.Specialization.Value);
}
