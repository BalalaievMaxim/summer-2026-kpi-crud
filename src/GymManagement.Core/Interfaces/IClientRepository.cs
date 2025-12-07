using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IClientRepository
{
    public Task<Client> GetClientByIdAsync(int clientId);
}