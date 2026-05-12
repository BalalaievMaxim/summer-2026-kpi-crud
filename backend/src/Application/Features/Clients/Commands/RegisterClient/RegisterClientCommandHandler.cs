using GymManagement.Application.Abstractions.Logging;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Features.Clients.Commands.RegisterClient;

public sealed class RegisterClientCommandHandler(
    IClientRepository clientRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    INotificationService notificationService,
    IAppLogger<RegisterClientCommandHandler> logger) : ICommandHandler<RegisterClientCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RegisterClientCommand command, CancellationToken cancellationToken = default)
    {
        if (await clientRepository.ExistsByEmailAsync(command.Email, cancellationToken: cancellationToken))
            throw new ClientEmailAlreadyExistsError(command.Email);

        var client = Client.Create(command.Name, command.Email, command.Phone, command.Password, passwordHasher);
        var clientId = await clientRepository.AddAsync(client, cancellationToken);

        var token = tokenService.CreateToken(clientId, client.Email.Value, "Client");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await notificationService.SendEmailAsync(
                client.Email.Value,
                "Welcome to IronPulse Gym!",
                $"Hello {client.Name.Value}, your account is successfully created.",
                timeoutCts.Token);
        }
        catch (NotificationException ex)
        {
            logger.LogWarning("Synchronous notification failed for client {0}. Reason: {1}", clientId, ex.Message);
        }
        catch (OperationCanceledException ex) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Synchronous notification timed out for client {0}. Reason: {1}", clientId, ex.Message);
        }

        return new AuthResultDto(
            clientId,
            client.Name.Value,
            client.Email.Value,
            client.Phone.Value,
            token
        );
    }
}