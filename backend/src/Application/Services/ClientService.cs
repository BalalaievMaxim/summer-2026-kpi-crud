using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using IClientAnalyticsRepository = GymManagement.Infrastructure.Persistence.Repositories.Interfaces.IClientAnalyticsRepository;

namespace GymManagement.Application.Services;

public class ClientService(
    IClientRepository clientRepository,
    IClientAnalyticsRepository analyticsRepository,
    IUnitOfWork unitOfWork) : IClientService
{
    public async Task<Client> RegisterClientAsync(CreateClientDto dto)
    {
        if (await clientRepository.ExistsByEmailAsync(dto.Email))
            throw new ClientEmailAlreadyExistsError(dto.Email);

        var client = Client.Create(dto.Name, dto.Email, dto.Phone, dto.Password);
        return await clientRepository.AddAsync(client);
    }

    public async Task<Client> LoginClientAsync(string email, string password)
    {
        var client = await clientRepository.GetByEmailAsync(email)
            ?? throw new InvalidCredentialsError();

        if (!client.MatchesPassword(password))
            throw new InvalidCredentialsError();

        return client;
    }

    public async Task UpdateClientAsync(int clientId, UpdateClientDto dto)
    {
        var client = await clientRepository.GetByIdAsync(clientId)
            ?? throw new ClientNotFoundError(clientId);

        if (!string.Equals(client.Email.Value, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await clientRepository.ExistsByEmailAsync(dto.Email, excludeId: clientId))
                throw new ClientEmailAlreadyExistsError(dto.Email);
        }

        client.UpdateContact(dto.Email, dto.Phone);

        await clientRepository.UpdateAsync(client);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteClientAsync(int clientId)
    {
        if (!await clientRepository.ExistsAsync(clientId))
            throw new ClientNotFoundError(clientId);

        if (await clientRepository.HasActiveEnrollmentsAsync(clientId))
            throw new ClientHasActiveEnrollmentsError(clientId);

        if (await clientRepository.HasActiveMembershipsAsync(clientId))
            throw new ClientHasActiveMembershipsError(clientId);

        await clientRepository.DeleteAsync(clientId);
        await unitOfWork.SaveChangesAsync();
    }

    public Task<Client?> GetByIdAsync(int clientId)
        => clientRepository.GetByIdAsync(clientId);

    public Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm)
        => clientRepository.SearchAsync(searchTerm);

    public Task<List<ClientActivityDto>> GetClientActivityAnalyticsAsync()
        => analyticsRepository.GetClientActivityAnalyticsAsync();
}
