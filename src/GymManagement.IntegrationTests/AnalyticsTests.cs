using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;
using Xunit;

namespace GymManagement.IntegrationTests;

public class AnalyticsTests : BaseIntegrationTest
{
    public AnalyticsTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task RevenueAnalytics_Should_Aggregate_Correctly()
    {
        var plan = new MembershipPlan { Name = "Gold", Price = 100, DurationMonths = 1 };
        var client = new Client { Name = "Client", Email = "c@test.com", Phone = "000", Password = "p" };
        Context.AddRange(plan, client);
        await Context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var membership = new Membership 
        { 
            ClientId = client.ClientId, 
            PlanId = plan.PlanId, 
            StartDate = today.AddDays(-5), 
            EndDate = today.AddMonths(1), 
            IsActive = true 
        };
        Context.Memberships.Add(membership);

        Context.Invoices.AddRange(
            new Invoice { ClientId = client.ClientId, Amount = 100, Status = "paid", Date = today, PaymentMethod = "card" },
            new Invoice { ClientId = client.ClientId, Amount = 100, Status = "paid", Date = today, PaymentMethod = "card" }
        );
        Context.Invoices.Add(
            new Invoice { ClientId = client.ClientId, Amount = 500, Status = "pending", Date = today }
        );

        await Context.SaveChangesAsync();

        var result = await Client.GetFromJsonAsync<List<TotalMembershipRevenueDto>>("/api/v1/invoices/analytics/revenue-by-plan");

        result.Should().NotBeNull();
        var report = result!.FirstOrDefault(r => r.PlanName == "Gold");
        
        report.Should().NotBeNull();
        report!.TotalRevenue.Should().Be(200);
    }
}