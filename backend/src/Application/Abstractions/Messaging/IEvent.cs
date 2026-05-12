namespace GymManagement.Application.Abstractions.Messaging;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}