using GymManagement.Application.Features.Enrollments.Events;
using GymManagement.Application.Features.Enrollments.Events.Handlers;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Infrastructure.Messaging;
using GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace GymManagement.Tests.Integration.Infrastructure.Messaging;

public class IdempotencyTests(GymApiFactory factory) : BaseIntegrationTest(factory)
{

    // Створюємо фейковий сервіс для перевірки кількості викликів відправки email
    private class FakeNotificationService : INotificationService
    {
        public int SendEmailCallCount { get; private set; }

        public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            SendEmailCallCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendEmail_WhenEnrollmentEventIsAlreadyProcessed()
    {
        // Arrange
        // Ініціалізуємо подію стандартним конструктором. 
        // EventId та OccurredOn згенеруються автоматично усередині рекорду.
        var @event = new EnrollmentCreatedEvent(1, "test@example.com", "Test Client", 1, DateTime.UtcNow);
        var eventId = @event.EventId;

        // Створюємо Outbox повідомлення, яке ВЖЕ має дату обробки (ProcessedOn), 
        // використовуючи згенерований ID події
        var outboxMessage = new OutboxMessage
        {
            Id = eventId,
            OccurredOn = @event.OccurredOn,
            Type = nameof(EnrollmentCreatedEvent),
            Content = "{}",
            ProcessedOn = DateTime.UtcNow 
        };

        Context.OutboxMessages.Add(outboxMessage);
        await Context.SaveChangesAsync();

        var fakeNotificationService = new FakeNotificationService();
        var innerHandler = new NotifyClientOnEnrollmentHandler(fakeNotificationService);
        var logger = NullLogger<IdempotentEventHandlerDecorator<EnrollmentCreatedEvent>>.Instance;

        var decorator = new IdempotentEventHandlerDecorator<EnrollmentCreatedEvent>(
            innerHandler,
            Context,
            logger
        );

        // Act
        await decorator.HandleAsync(@event);

        // Assert
        // Очікуємо 0 викликів, оскільки декоратор має зупинити виконання
        Assert.Equal(0, fakeNotificationService.SendEmailCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldSendEmail_WhenEnrollmentEventIsNotProcessed()
    {
        // Arrange
        var @event = new EnrollmentCreatedEvent(1, "test@example.com", "Test Client", 1, DateTime.UtcNow);
        var eventId = @event.EventId;

        // Створюємо Outbox повідомлення, яке ЩЕ НЕ оброблене (ProcessedOn == null)
        var outboxMessage = new OutboxMessage
        {
            Id = eventId,
            OccurredOn = @event.OccurredOn,
            Type = nameof(EnrollmentCreatedEvent),
            Content = "{}",
            ProcessedOn = null 
        };

        Context.OutboxMessages.Add(outboxMessage);
        await Context.SaveChangesAsync();

        var fakeNotificationService = new FakeNotificationService();
        var innerHandler = new NotifyClientOnEnrollmentHandler(fakeNotificationService);
        var logger = NullLogger<IdempotentEventHandlerDecorator<EnrollmentCreatedEvent>>.Instance;

        var decorator = new IdempotentEventHandlerDecorator<EnrollmentCreatedEvent>(
            innerHandler,
            Context,
            logger
        );

        // Act
        await decorator.HandleAsync(@event);

        // Assert
        // Очікуємо 1 виклик, оскільки декоратор дозволяє виконання
        Assert.Equal(1, fakeNotificationService.SendEmailCallCount);
    }
    
    [Fact]
    public async Task HandleAsync_ShouldSendEmail_WhenOutboxMessageIsNotFound()
    {
        // Arrange
        var @event = new EnrollmentCreatedEvent(1, "test@example.com", "Test Client", 1, DateTime.UtcNow);

        // У цьому тесті ми НАВМИСНО не додаємо OutboxMessage в базу (симуляція відсутності)
        
        var fakeNotificationService = new FakeNotificationService();
        var innerHandler = new NotifyClientOnEnrollmentHandler(fakeNotificationService);
        var logger = NullLogger<IdempotentEventHandlerDecorator<EnrollmentCreatedEvent>>.Instance;

        var decorator = new IdempotentEventHandlerDecorator<EnrollmentCreatedEvent>(
            innerHandler,
            Context,
            logger
        );

        // Act
        await decorator.HandleAsync(@event);

        // Assert
        // Очікуємо 1 виклик, бо декоратор пропускає перевірку, якщо повідомлення не знайдено
        Assert.Equal(1, fakeNotificationService.SendEmailCallCount);
    }
}