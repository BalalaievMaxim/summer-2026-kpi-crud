using GymManagement.Application.DTOs;
using GymManagement.Domain.Billing;
using GymManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
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

    protected async Task SeedActiveMembershipAsync(int clientId, int planId)
    {
        var purchaseResponse = await Client.PostAsJsonAsync("/api/v1/memberships", new PurchaseMembershipDto
        {
            ClientId = clientId,
            PlanId = planId,
            PaymentMethod = PaymentMethod.Cash
        });
        purchaseResponse.EnsureSuccessStatusCode();

        var invoice = await Context.Invoices
            .OrderByDescending(i => i.InvoiceId)
            .FirstAsync(i => i.ClientId == clientId);

        var payResponse = await Client.PutAsync($"/api/v1/invoices/{invoice.InvoiceId}/pay", null);
        payResponse.EnsureSuccessStatusCode();

        await Context.Entry(invoice).ReloadAsync();
    }

    protected async Task SeedPendingMembershipAsync(int clientId, int planId)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/memberships", new PurchaseMembershipDto
        {
            ClientId = clientId,
            PlanId = planId,
            PaymentMethod = PaymentMethod.Cash
        });
        response.EnsureSuccessStatusCode();
    }
}