using FluentAssertions;
using GymManagement.Application.Services;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Application.Services.Interfaces;
using Moq;
using Xunit;

namespace GymManagement.Tests.Unit.Services;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _clientRepoMock = new Mock<IClientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ClientService(_clientRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateClient_EmailAlreadyExists_Should_ThrowException()
    {
        var clientId = 1;
        var existingClient = new Client { ClientId = clientId, Email = "old@test.com" };
        var updateDto = new UpdateClientDto { Email = "taken@test.com", Phone = "123" };

        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(existingClient);
        _clientRepoMock.Setup(r => r.ExistsWithEmailAsync(updateDto.Email, clientId)).ReturnsAsync(true);

        var act = async () => await _service.UpdateClientAsync(clientId, updateDto);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Email is already in use by another client.");
                 
        _clientRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClient_ValidData_Should_UpdateAndSave()
    {
        var clientId = 1;
        var existingClient = new Client { ClientId = clientId, Email = "old@test.com" };
        var updateDto = new UpdateClientDto { Email = "new@test.com", Phone = "123" };

        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(existingClient);
        _clientRepoMock.Setup(r => r.ExistsWithEmailAsync(updateDto.Email, clientId)).ReturnsAsync(false);

        await _service.UpdateClientAsync(clientId, updateDto);

        existingClient.Email.Should().Be("new@test.com");
        _clientRepoMock.Verify(r => r.UpdateAsync(existingClient), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }
    [Fact]
    public async Task DeleteClient_ValidId_Should_RemoveAndSave()
    {
        var client = new Client { ClientId = 1 };
        _clientRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        await _service.DeleteClientAsync(1);

        _clientRepoMock.Verify(r => r.RemoveAsync(client), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(default), Times.Once);
    }
}