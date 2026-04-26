using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.DTOs;
using GymManagement.Models;
using GymManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GymManagement.Tests.Integration;

public class MembershipFlowTests : BaseIntegrationTest
{
    public MembershipFlowTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task FullCycle_Purchase_And_Pay_Should_ActivateMembership()
    {
        var plan = new MembershipPlan { Name = "Full Access", Price = 1000, DurationMonths = 1 };
        var client = new Client { Name = "John Doe", Email = "john@test.com", Phone = "555-000", Password = "pass" };
        
        Context.Membershipplans.Add(plan);
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        var purchaseDto = new PurchaseMembershipDto
        {
            ClientId = client.ClientId,
            PlanId = plan.PlanId,
            PaymentMethod = PaymentMethod.Card,
            Notes = "Test Purchase"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/memberships", purchaseDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var invoice = await Context.Invoices.FirstAsync(i => i.ClientId == client.ClientId);
        var membership = await Context.Memberships.FirstAsync(m => m.ClientId == client.ClientId);

        invoice.Status.Should().Be("pending");
        membership.IsActive.Should().BeFalse();

        var payResponse = await Client.PutAsync($"/api/v1/invoices/{invoice.InvoiceId}/pay", null);
        payResponse.EnsureSuccessStatusCode();

        await Context.Entry(invoice).ReloadAsync();
        await Context.Entry(membership).ReloadAsync();

        invoice.Status.Should().Be("paid");
        membership.IsActive.Should().BeTrue();
    }
}