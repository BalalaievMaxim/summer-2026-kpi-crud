using System.Text.Json;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Infrastructure.Persistence;
using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Infrastructure.Messaging;

public sealed class OutboxEventBus(GymManagementContext context) : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        var outboxMessage = new OutboxMessage
        {
            Id = @event.EventId,
            Type = @event.GetType().AssemblyQualifiedName ?? @event.GetType().Name,
            Content = JsonSerializer.Serialize(@event),
            OccurredOn = @event.OccurredOn
        };

        context.OutboxMessages.Add(outboxMessage);
        
        return Task.CompletedTask;
    }
}