using FluentAssertions;
using GymManagement.Application.Abstractions.Logging;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Auth.Queries.LoginClient;
using GymManagement.Application.Features.Clients.Commands.DeleteClient;
using GymManagement.Application.Features.Clients.Commands.RegisterClient;
using GymManagement.Application.Features.Clients.Commands.UpdateClient;
using GymManagement.Application.Features.Coaches.Commands.DeleteCoach;
using GymManagement.Application.Features.Coaches.Commands.RegisterCoach;
using GymManagement.Application.Features.Coaches.Commands.UpdateCoachSpecialization;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Ports;
using Moq;

namespace GymManagement.Tests.Unit.Features.Clients;

public sealed class ClientsAndCoachesHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<ICoachRepository> _coachRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly TestPasswordHasher _passwordHasher = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IAppLogger<RegisterClientCommandHandler>> _loggerMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    [Fact]
    public async Task RegisterClient_EmailFree_Should_CreateAndReturnId()
    {
        var handler = new RegisterClientCommandHandler(
            _clientRepoMock.Object, 
            _passwordHasher, 
            _tokenServiceMock.Object,
            _notificationServiceMock.Object, 
            _loggerMock.Object);

        _clientRepoMock.Setup(r => r.ExistsByEmailAsync("john@test.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);
            
        _tokenServiceMock.Setup(ts => ts.CreateToken(7, "john@test.com", "Client"))
            .Returns("mocked-jwt-token");

        var result = await handler.Handle(new RegisterClientCommand("John", "john@test.com", "+380671234567", "pass1234"));

        result.ClientId.Should().Be(7);
        result.Token.Should().Be("mocked-jwt-token"); 
        
        _clientRepoMock.Verify(r => r.AddAsync(It.Is<Client>(client =>
            client.Name.Value == "John" &&
            client.Email.Value == "john@test.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterClient_EmailTaken_Should_ThrowDomainError()
    {
        var handler = new RegisterClientCommandHandler(
            _clientRepoMock.Object, 
            _passwordHasher, 
            _tokenServiceMock.Object,
            _notificationServiceMock.Object, 
            _loggerMock.Object);

        _clientRepoMock.Setup(r => r.ExistsByEmailAsync("taken@test.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await handler.Handle(new RegisterClientCommand("John", "taken@test.com", "+380671234567", "pass1234"));

        await act.Should().ThrowAsync<ClientEmailAlreadyExistsError>();
    }

    [Fact]
    public async Task RegisterClient_NotificationFails_Should_CreateClientAndNotThrow()
    {
        var handler = new RegisterClientCommandHandler(
            _clientRepoMock.Object, 
            _passwordHasher, 
            _tokenServiceMock.Object,
            _notificationServiceMock.Object, 
            _loggerMock.Object);

        _clientRepoMock.Setup(r => r.ExistsByEmailAsync("john@test.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);
            
        _tokenServiceMock.Setup(ts => ts.CreateToken(7, "john@test.com", "Client"))
            .Returns("mocked-jwt-token");

        _notificationServiceMock.Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotificationException("SMTP Server Down"));

        var result = await handler.Handle(new RegisterClientCommand("John", "john@test.com", "+380671234567", "pass1234"));

        result.ClientId.Should().Be(7);
        result.Token.Should().Be("mocked-jwt-token");
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterClient_NotificationTimeOut_Should_CreateClientAndNotThrow()
    {
        var handler = new RegisterClientCommandHandler(
            _clientRepoMock.Object, 
            _passwordHasher, 
            _tokenServiceMock.Object,
            _notificationServiceMock.Object, 
            _loggerMock.Object);

        _clientRepoMock.Setup(r => r.ExistsByEmailAsync("john@test.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);
            
        _tokenServiceMock.Setup(ts => ts.CreateToken(7, "john@test.com", "Client"))
            .Returns("mocked-jwt-token");

        _notificationServiceMock.Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("Timeout"));

        var result = await handler.Handle(new RegisterClientCommand("John", "john@test.com", "+380671234567", "pass1234"));

        result.ClientId.Should().Be(7);
        result.Token.Should().Be("mocked-jwt-token");
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginClient_ValidCredentials_Should_ReturnClientDto()
    {
        var handler = new LoginClientQueryHandler(_clientRepoMock.Object, _passwordHasher, _tokenServiceMock.Object);
        var client = Client.Reconstitute(1, "John", "john@test.com", "+380671234567", _passwordHasher.Hash("pass1234"));

        _clientRepoMock.Setup(r => r.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var result = await handler.Handle(new LoginClientQuery("john@test.com", "pass1234"));

        result.ClientId.Should().Be(1);
        result.Email.Should().Be("john@test.com");
    }

    [Fact]
    public async Task LoginClient_WrongPassword_Should_ThrowDomainError()
    {
        var handler = new LoginClientQueryHandler(_clientRepoMock.Object, _passwordHasher, _tokenServiceMock.Object);
        var client = Client.Reconstitute(1, "John", "john@test.com", "+380671234567", _passwordHasher.Hash("pass1234"));

        _clientRepoMock.Setup(r => r.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var act = async () => await handler.Handle(new LoginClientQuery("john@test.com", "wrongpass"));

        await act.Should().ThrowAsync<InvalidCredentialsError>();
    }

    [Fact]
    public async Task UpdateClient_ValidData_Should_UpdateContactAndSave()
    {
        var handler = new UpdateClientCommandHandler(_clientRepoMock.Object, _unitOfWorkMock.Object);
        var client = Client.Reconstitute(1, "John", "old@test.com", "+380671234567", "pass");

        _clientRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.ExistsByEmailAsync("new@test.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await handler.Handle(new UpdateClientCommand(1, "new@test.com", "+380679999999"));

        client.Email.Value.Should().Be("new@test.com");
        client.Phone.Value.Should().Be("+380679999999");
        _clientRepoMock.Verify(r => r.UpdateAsync(client, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClient_Exists_Should_DeleteAndSave()
    {
        var handler = new DeleteClientCommandHandler(_clientRepoMock.Object, _unitOfWorkMock.Object);

        _clientRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await handler.Handle(new DeleteClientCommand(1));

        _clientRepoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClient_NotFound_Should_ThrowDomainError()
    {
        var handler = new DeleteClientCommandHandler(_clientRepoMock.Object, _unitOfWorkMock.Object);

        _clientRepoMock.Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await handler.Handle(new DeleteClientCommand(99));

        await act.Should().ThrowAsync<ClientNotFoundError>();
    }

    [Fact]
    public async Task RegisterCoach_EmailFree_Should_CreateAndReturnId()
    {
        var handler = new RegisterCoachCommandHandler(_coachRepoMock.Object, _passwordHasher);

        _coachRepoMock.Setup(r => r.ExistsByEmailAsync("coach@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _coachRepoMock.Setup(r => r.AddAsync(It.IsAny<Coach>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var result = await handler.Handle(new RegisterCoachCommand("Coach", "coach@test.com", "Yoga", "pass1234"));

        result.Should().Be(9);
        _coachRepoMock.Verify(r => r.AddAsync(It.Is<Coach>(coach =>
            coach.Name.Value == "Coach" &&
            coach.Email.Value == "coach@test.com" &&
            coach.Specialization.Value == "Yoga"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCoachSpecialization_ValidData_Should_UpdateAndSave()
    {
        var handler = new UpdateCoachSpecializationCommandHandler(_coachRepoMock.Object, _unitOfWorkMock.Object);
        var coach = Coach.Reconstitute(1, "Coach", "coach@test.com", "Yoga", "pass");

        _coachRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(coach);

        await handler.Handle(new UpdateCoachSpecializationCommand(1, "Pilates"));

        coach.Specialization.Value.Should().Be("Pilates");
        _coachRepoMock.Verify(r => r.UpdateAsync(coach, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCoach_WithEnrolledUpcomingClasses_Should_ThrowDomainError()
    {
        var handler = new DeleteCoachCommandHandler(_coachRepoMock.Object, _unitOfWorkMock.Object);

        _coachRepoMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _coachRepoMock.Setup(r => r.HasUpcomingClassesWithEnrollmentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await handler.Handle(new DeleteCoachCommand(1));

        await act.Should().ThrowAsync<CoachHasFutureClassesError>();
        _coachRepoMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}