using System.Text.Json;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Messaging;

public sealed class EventDispatcherBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<EventDispatcherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Event Dispatcher Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error in Outbox Event Dispatcher.");
            }

            await Task.Delay(3000, stoppingToken);
        }
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GymManagementContext>();

        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    throw new InvalidOperationException($"Event type {message.Type} not found.");
                }

                var @event = JsonSerializer.Deserialize(message.Content, eventType) as IEvent;
                if (@event is null)
                {
                    throw new InvalidOperationException("Failed to deserialize event.");
                }

                var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                var handlers = scope.ServiceProvider.GetServices(handlerType).ToList();

                foreach (var handler in handlers)
                {
                    if (handler is null) continue;
                    
                    dynamic dynamicHandler = handler;
                    await dynamicHandler.HandleAsync((dynamic)@event, stoppingToken);
                }

                message.ProcessedOn = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {MessageId}.", message.Id);
                message.Error = ex.Message;
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}