using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GymManagement.IntegrationTests;

[Collection("Integration")]
public abstract class BaseIntegrationTest : IClassFixture<GymApiFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly GymManagementContext Context;
    private readonly Func<Task> _resetDatabase;

    protected BaseIntegrationTest(GymApiFactory factory)
    {
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<GymManagementContext>();

        _resetDatabase = async () =>
        {
            await Context.Database.ExecuteSqlRawAsync(@"
                TRUNCATE TABLE ""invoice"", ""membership"", ""planaccess"", ""enrollment"", ""class"" CASCADE;
                TRUNCATE TABLE ""membershipplan"", ""client"", ""coach"", ""classtype"" CASCADE;
            ");
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
        Scope.Dispose();
    }
}