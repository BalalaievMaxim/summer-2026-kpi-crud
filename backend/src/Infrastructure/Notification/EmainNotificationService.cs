using GymManagement.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Notifications;

public sealed class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("START Sending email to {To}. Subject: {Subject}", to, subject);

        await Task.Delay(1500, cancellationToken);

        // Simulate a random failure with a 10% chance
        if (Random.Shared.NextDouble() < 0.1)
        {
            _logger.LogWarning("FAILED to send email to {To}. SMTP Server Timeout.", to);
            throw new Exception("SMTP Server timeout.");
        }

        _logger.LogInformation("SUCCESS Email sent to {To}.", to);
    }
}