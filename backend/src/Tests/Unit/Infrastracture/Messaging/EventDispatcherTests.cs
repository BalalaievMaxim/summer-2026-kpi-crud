using System.Text.Json;
using FluentAssertions;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Infrastructure.Messaging;
using GymManagement.Infrastructure.Persistence;
using GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Tests.Unit.Infrastructure.Messaging;

public sealed record DummyEvent(string Message) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed class EventDispatcherTests
{
    [Fact]
    public async Task ProcessOutboxMessages_Success_MarksAsProcessed()
    {
        // 1. Генеруємо фіксоване ім'я для БД на весь час виконання одного тесту
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        
        services.AddDbContext<GymManagementContext>(opts => opts.UseInMemoryDatabase(dbName));
        
        var handlerMock = new Mock<IEventHandler<DummyEvent>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<DummyEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        services.AddScoped<IEventHandler<DummyEvent>>(_ => handlerMock.Object);
        
        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<GymManagementContext>();
        
        var @event = new DummyEvent("Test");
        var outboxMsg = new OutboxMessage
        {
            Id = @event.EventId,
            Type = typeof(DummyEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(@event),
            OccurredOn = @event.OccurredOn
        };
        
        context.OutboxMessages.Add(outboxMsg);
        await context.SaveChangesAsync();
        
        var dispatcher = new EventDispatcherBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(), 
            Mock.Of<ILogger<EventDispatcherBackgroundService>>());
        
        await dispatcher.ProcessOutboxMessagesAsync(CancellationToken.None);
        
        context.ChangeTracker.Clear();
        var processedMsg = await context.OutboxMessages.FirstAsync();
        
        processedMsg.ProcessedOn.Should().NotBeNull();
        processedMsg.Error.Should().BeNull();
        
        handlerMock.Verify(h => h.HandleAsync(It.IsAny<DummyEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOutboxMessages_Failure_SavesErrorAndLeavesUnprocessed()
    {
        // 1. Фіксоване ім'я БД для другого тесту
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        
        services.AddDbContext<GymManagementContext>(opts => opts.UseInMemoryDatabase(dbName));
        
        var handlerMock = new Mock<IEventHandler<DummyEvent>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<DummyEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Handler failed"));
        
        services.AddScoped<IEventHandler<DummyEvent>>(_ => handlerMock.Object);
        
        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<GymManagementContext>();
        
        var @event = new DummyEvent("Test");
        var outboxMsg = new OutboxMessage
        {
            Id = @event.EventId,
            Type = typeof(DummyEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(@event),
            OccurredOn = @event.OccurredOn
        };
        
        context.OutboxMessages.Add(outboxMsg);
        await context.SaveChangesAsync();
        
        var dispatcher = new EventDispatcherBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(), 
            Mock.Of<ILogger<EventDispatcherBackgroundService>>());
        
        await dispatcher.ProcessOutboxMessagesAsync(CancellationToken.None);
        
        context.ChangeTracker.Clear();
        var processedMsg = await context.OutboxMessages.FirstAsync();
        
        processedMsg.ProcessedOn.Should().BeNull();
        processedMsg.Error.Should().Be("Handler failed");
    }
}