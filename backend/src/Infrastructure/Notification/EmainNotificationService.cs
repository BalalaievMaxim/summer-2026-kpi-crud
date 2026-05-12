using GymManagement.Application.Exceptions;
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

        try
        {
            await Task.Delay(1500, cancellationToken);

            if (Random.Shared.NextDouble() < 0.1)
            {
                throw new TimeoutException("SMTP Server timeout.");
            }

            _logger.LogInformation("SUCCESS Email sent to {To}.", to);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FAILED to send email to {To}.", to);
            throw new NotificationException($"Failed to send email to {to}", ex);
        }
    }
}