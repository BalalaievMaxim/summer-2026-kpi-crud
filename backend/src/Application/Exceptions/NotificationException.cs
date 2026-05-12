using System;

namespace GymManagement.Application.Exceptions;

public class NotificationException : Exception
{
    public NotificationException(string message) : base(message) { }
    
    public NotificationException(string message, Exception innerException) : base(message, innerException) { }
}