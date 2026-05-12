using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Domain.Clients;

namespace GymManagement.Application.Features.Clients;

internal static class ClientMappings
{
    public static ClientDto ToDto(Client client)
        => new(client.Id, client.Name.Value, client.Email.Value, client.Phone.Value);

    public static ClientSummaryDto ToSummaryDto(Client client)
        => new(client.Id, client.Name.Value, client.Email.Value);
}
