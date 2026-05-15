using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Domain.Billing;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using Xunit;

namespace GymManagement.Tests.Integration;

public class ErrorHandlingTests : BaseIntegrationTest
{
    public ErrorHandlingTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateInvoice_ForNonExistentClient_Should_ReturnNotFound()
    {
        var plan = new MembershipPlan { Name = "Test", Price = 10, DurationMonths = 1 };
        Context.Membershipplans.Add(plan);
        await Context.SaveChangesAsync();

        var dto = new CreateInvoiceRequestDto
        {
            ClientId = 99999,
            MembershipPlanId = plan.PlanId,
            PaymentMethod = PaymentMethod.Cash
        };

        var response = await Client.PostAsJsonAsync("/api/v1/invoices/create", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PurchaseMembership_WhenAlreadyActive_Should_ReturnConflict()
    {
        var plan = new MembershipPlan { Name = "Test", Price = 10, DurationMonths = 1 };
        var client = new Client { Name = "C", Email = "e", Phone = "p", Password = "p" };
        Context.AddRange(plan, client);
        await Context.SaveChangesAsync();

        Context.Memberships.Add(new Membership 
        { 
            ClientId = client.ClientId, 
            PlanId = plan.PlanId, 
            IsActive = true, 
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        });
        await Context.SaveChangesAsync();

        var dto = new PurchaseMembershipDto
        {
            ClientId = client.ClientId,
            PlanId = plan.PlanId,
            PaymentMethod = PaymentMethod.Cash
        };

        var response = await Client.PostAsJsonAsync("/api/v1/memberships", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}