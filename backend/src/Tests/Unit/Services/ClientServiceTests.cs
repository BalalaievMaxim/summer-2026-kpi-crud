using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Application.Services;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using Moq;

namespace GymManagement.Tests.Unit.Services;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<IClientAnalyticsRepository> _analyticsMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _service = new ClientService(_clientRepoMock.Object, _analyticsMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task RegisterClientAsync_EmailFree_CreatesAndReturnsClient()
    {
        var dto = new CreateClientDto { Name = "John", Email = "john@test.com", Phone = "+380671234567", Password = "pass1234" };
        var domainClient = Client.Reconstitute(1, dto.Name, dto.Email, dto.Phone, dto.Password);

        _clientRepoMock.Setup(r => r.ExistsByEmailAsync(dto.Email, null, default)).ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>(), default)).ReturnsAsync(domainClient);

        var result = await _service.RegisterClientAsync(dto);

        result.Id.Should().Be(1);
        result.Email.Value.Should().Be(dto.Email);
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), default), Times.Once);
    }

    [Fact]
    public async Task RegisterClientAsync_EmailTaken_ThrowsClientEmailAlreadyExistsError()
    {
        var dto = new CreateClientDto { Name = "John", Email = "taken@test.com", Phone = "+380671234567", Password = "pass1234" };
        _clientRepoMock.Setup(r => r.ExistsByEmailAsync(dto.Email, null, default)).ReturnsAsync(true);

        var act = async () => await _service.RegisterClientAsync(dto);
        await act.Should().ThrowAsync<ClientEmailAlreadyExistsError>();
    }

    [Fact]
    public async Task LoginClientAsync_ValidCredentials_ReturnsClient()
    {
        var client = Client.Reconstitute(1, "John", "john@test.com", "+380671234567", "pass1234");
        _clientRepoMock.Setup(r => r.GetByEmailAsync("john@test.com", default)).ReturnsAsync(client);

        var result = await _service.LoginClientAsync("john@test.com", "pass1234");

        result.Should().Be(client);
    }

    [Fact]
    public async Task LoginClientAsync_WrongPassword_ThrowsInvalidCredentialsError()
    {
        var client = Client.Reconstitute(1, "John", "john@test.com", "+380671234567", "pass1234");
        _clientRepoMock.Setup(r => r.GetByEmailAsync("john@test.com", default)).ReturnsAsync(client);

        var act = async () => await _service.LoginClientAsync("john@test.com", "wrongpass");
        await act.Should().ThrowAsync<InvalidCredentialsError>();
    }

    [Fact]
    public async Task LoginClientAsync_UnknownEmail_ThrowsInvalidCredentialsError()
    {
        _clientRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((Client?)null);

        var act = async () => await _service.LoginClientAsync("nobody@test.com", "pass");
        await act.Should().ThrowAsync<InvalidCredentialsError>();
    }

    [Fact]
    public async Task UpdateClientAsync_ValidData_CallsUpdateContactAndSaves()
    {
        var client = Client.Reconstitute(1, "John", "old@test.com", "+380671234567", "pass");
        var dto = new UpdateClientDto { Email = "new@test.com", Phone = "+380679999999" };

        _clientRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.ExistsByEmailAsync(dto.Email, 1, default)).ReturnsAsync(false);

        await _service.UpdateClientAsync(1, dto);

        client.Email.Value.Should().Be("new@test.com");
        client.Phone.Value.Should().Be("+380679999999");
        _clientRepoMock.Verify(r => r.UpdateAsync(client, default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_ClientNotFound_ThrowsClientNotFoundError()
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Client?)null);

        var act = async () => await _service.UpdateClientAsync(99, new UpdateClientDto { Email = "e@e.com", Phone = "+380671111111" });
        await act.Should().ThrowAsync<ClientNotFoundError>();
    }

    [Fact]
    public async Task UpdateClientAsync_EmailTakenByOther_ThrowsClientEmailAlreadyExistsError()
    {
        var client = Client.Reconstitute(1, "John", "old@test.com", "+380671234567", "pass");
        var dto = new UpdateClientDto { Email = "taken@test.com", Phone = "+380671234567" };

        _clientRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.ExistsByEmailAsync(dto.Email, 1, default)).ReturnsAsync(true);

        var act = async () => await _service.UpdateClientAsync(1, dto);
        await act.Should().ThrowAsync<ClientEmailAlreadyExistsError>();
    }

    [Fact]
    public async Task DeleteClientAsync_Exists_DeletesAndSaves()
    {
        _clientRepoMock.Setup(r => r.ExistsAsync(1, default)).ReturnsAsync(true);

        await _service.DeleteClientAsync(1);

        _clientRepoMock.Verify(r => r.DeleteAsync(1, default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteClientAsync_NotFound_ThrowsClientNotFoundError()
    {
        _clientRepoMock.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        var act = async () => await _service.DeleteClientAsync(99);
        await act.Should().ThrowAsync<ClientNotFoundError>();
    }
}
