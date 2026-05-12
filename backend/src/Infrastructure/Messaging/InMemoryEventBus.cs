using System.Threading.Channels;
using GymManagement.Application.Abstractions.Messaging;

namespace GymManagement.Infrastructure.Messaging;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly Channel<IEvent> _channel = Channel.CreateUnbounded<IEvent>();

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }

    public IAsyncEnumerable<IEvent> ReadAllEventsAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}