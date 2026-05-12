namespace GymManagement.Application.Abstractions.Messaging;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}