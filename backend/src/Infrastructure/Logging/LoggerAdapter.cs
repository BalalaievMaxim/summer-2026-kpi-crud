using GymManagement.Application.Abstractions.Logging;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Logging;

public sealed class LoggerAdapter<T>(ILogger<T> logger) : IAppLogger<T>
{
    public void LogInformation(string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        logger.LogWarning(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        logger.LogError(exception, message, args);
    }
}