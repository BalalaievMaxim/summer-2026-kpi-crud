namespace GymManagement.Application.Abstractions.Messaging;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}