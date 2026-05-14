using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Messaging;

public sealed class IdempotentEventHandlerDecorator<TEvent>(
    IEventHandler<TEvent> decorated,
    GymManagementContext context,
    ILogger<IdempotentEventHandlerDecorator<TEvent>> logger
) : IEventHandler<TEvent> where TEvent : IEvent
{
    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        var outboxMessage = await context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == @event.EventId, cancellationToken);

        if (outboxMessage is null)
        {
            logger.LogWarning("Outbox message {EventId} not found. Skipping idempotency check.", @event.EventId);
            await decorated.HandleAsync(@event, cancellationToken);
            return;
        }

        if (outboxMessage.ProcessedOn is not null)
        {
            logger.LogInformation("Event {EventId} was already processed at {Time}. Skipping.",
                @event.EventId, outboxMessage.ProcessedOn);
            return;
        }

        await decorated.HandleAsync(@event, cancellationToken);
    }
}