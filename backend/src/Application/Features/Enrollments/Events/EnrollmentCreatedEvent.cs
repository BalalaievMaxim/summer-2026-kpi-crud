using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Enrollments.Events;

public sealed record EnrollmentCreatedEvent(
    int ClientId,
    int ClassId,
    DateTime RegistrationTime) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}