using GymManagement.Core.Interfaces;
using System;
using System.Threading.Tasks;
using GymManagement.Core.Entities;
using System.Collections.Generic;
using GymManagement.Core.DTOs;

namespace GymManagement.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IClientRepository clientRepository, IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateClientAsync(int clientId, UpdateClientDto updateClientDto)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        if (client.Email != updateClientDto.Email)
        {
            var emailExists = await _clientRepository.ExistsWithEmailAsync(updateClientDto.Email, clientId);
            if (emailExists)
            {
                throw new InvalidOperationException("Email is already in use by another client.");
            }
        }

        client.Email = updateClientDto.Email;
        client.Phone = updateClientDto.Phone;

        await _clientRepository.UpdateAsync(client);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteClientAsync(int clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        await _clientRepository.RemoveAsync(client);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Client> GetClientClassHistoryAsync(int clientId)
    {
        var client = await _clientRepository.GetByIdWithEnrollmentsAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException("Client not found.");
        }
        return client;
    }

    public async Task<List<Client>> SearchClientsAsync(string searchTerm)
    {
        return await _clientRepository.SearchByNameOrEmailAsync(searchTerm);
    }

    public Task<List<ClientActivityDto>> GetClientActivityAnalyticsAsync()
    {
        return _clientRepository.GetClientActivityAnalyticsAsync();
    }

    public async Task<Client> RegisterClientAsync(CreateClientDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 4)
            throw new ArgumentException("Password must be at least 4 characters.");

        var existing = await _clientRepository.SearchByNameOrEmailAsync(dto.Email);
        if (existing.Any(c => c.Email == dto.Email))
            throw new InvalidOperationException("Email is already in use.");

        var client = new Client
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password, // plain text — без JWT це ок
            Phone = dto.Phone,
            CreatedAt = DateTime.UtcNow,
        };
        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();
        return client;
    }

    public async Task<Client> LoginClientAsync(string email, string password)
    {
        var results = await _clientRepository.SearchByNameOrEmailAsync(email);
        var client = results.FirstOrDefault(c => c.Email == email);
        if (client == null)
            throw new InvalidOperationException("User not found.");
        if (client.Password != password)
            throw new InvalidOperationException("Wrong password.");
        return client;
    }
}
