using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using Xunit;

namespace GymManagement.IntegrationTests;

public class DataRetrievalTests : BaseIntegrationTest
{
    public DataRetrievalTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetPendingInvoices_Should_ReturnOnly_Pending_Invoices()
    {
        var client = new Client { Name = "Debtor", Email = "debt@test.com", Phone = "999", Password = "p" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        Context.Invoices.AddRange(
            new Invoice { ClientId = client.ClientId, Amount = 100, Status = "paid" },   
            new Invoice { ClientId = client.ClientId, Amount = 200, Status = "pending" }, 
            new Invoice { ClientId = client.ClientId, Amount = 300, Status = "cancelled" }
        );
        await Context.SaveChangesAsync();

        var invoices = await Client.GetFromJsonAsync<List<InvoiceResponseDto>>($"/api/v1/invoices/pending/{client.ClientId}");

        invoices.Should().HaveCount(1);
        invoices![0].Status.Should().Be("pending");
        invoices![0].Amount.Should().Be(200);
    }

    [Fact]
    public async Task GetActiveMemberships_Should_Filter_Expired_And_Inactive()
    {
        var plan = new MembershipPlan { Name = "P", Price = 10, DurationMonths = 12 };
        var client = new Client { Name = "Gym Rat", Email = "gym@test.com", Phone = "888", Password = "p" };
        Context.AddRange(plan, client);
        await Context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Now);

        Context.Memberships.AddRange(
            new Membership { ClientId = client.ClientId, PlanId = plan.PlanId, IsActive = true, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(5) },
            
            new Membership { ClientId = client.ClientId, PlanId = plan.PlanId, IsActive = false, StartDate = today, EndDate = today.AddMonths(1) },
            
            new Membership { ClientId = client.ClientId, PlanId = plan.PlanId, IsActive = true, StartDate = today.AddMonths(-10), EndDate = today.AddDays(-1) }
        );
        await Context.SaveChangesAsync();

        var memberships = await Client.GetFromJsonAsync<List<Membership>>($"/api/v1/memberships/active/{client.ClientId}");

        memberships.Should().HaveCount(1);
        memberships![0].EndDate.Should().BeAfter(today);
        memberships![0].IsActive.Should().BeTrue();
    }
}