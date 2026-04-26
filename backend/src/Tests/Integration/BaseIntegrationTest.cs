using GymManagement.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GymManagement.Tests.Integration;

[Collection("Integration")]
public abstract class BaseIntegrationTest : IAsyncLifetime 
{
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly GymManagementContext Context;
    private readonly GymApiFactory _factory;

    protected BaseIntegrationTest(GymApiFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<GymManagementContext>();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        Scope.Dispose();
        return Task.CompletedTask;
    }
}