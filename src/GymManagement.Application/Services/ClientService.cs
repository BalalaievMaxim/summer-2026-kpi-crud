using GymManagement.Core.Interfaces;
using GymManagement.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace GymManagement.Application.Services;

public class ClientService
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
}
