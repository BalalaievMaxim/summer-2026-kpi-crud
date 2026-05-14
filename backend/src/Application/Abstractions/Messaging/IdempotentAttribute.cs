namespace GymManagement.Application.Abstractions.Messaging;

[AttributeUsage(AttributeTargets.Class)]
public sealed class IdempotentAttribute : Attribute { }