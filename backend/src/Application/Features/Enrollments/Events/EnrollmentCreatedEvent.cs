using System;
using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Application.Features.Enrollments.Events;

public sealed record EnrollmentCreatedEvent(
    int ClientId,
    string ClientEmail,
    string ClientName,
    int ClassId,
    DateTime RegistrationTime) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}