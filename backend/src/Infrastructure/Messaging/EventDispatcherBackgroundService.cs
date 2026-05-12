using GymManagement.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Messaging;

public sealed class EventDispatcherBackgroundService(
    InMemoryEventBus eventBus,
    IServiceScopeFactory scopeFactory,
    ILogger<EventDispatcherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Event Dispatcher Background Service started.");

        await foreach (var @event in eventBus.ReadAllEventsAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var eventType = @event.GetType();
                var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                
                var handlers = scope.ServiceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    if (handler is null) continue;
                    
                    var method = handlerType.GetMethod("HandleAsync");
                    if (method is not null)
                    {
                        var task = (Task)method.Invoke(handler, new object[] { @event, stoppingToken })!;
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while handling event {EventId} of type {EventType}", @event.EventId, @event.GetType().Name);
            }
        }
    }
}