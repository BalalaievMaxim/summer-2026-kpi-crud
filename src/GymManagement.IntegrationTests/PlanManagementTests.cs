using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GymManagement.IntegrationTests;

public class PlanManagementTests : BaseIntegrationTest
{
    public PlanManagementTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task CreatePlan_Should_SaveToDatabase()
    {
        var dto = new CreateMembershipPlanDto { Name = "Yoga", DurationMonth = 3, Price = 300 };

        var response = await Client.PostAsJsonAsync("/api/v1/membership-plans", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var plan = await Context.Membershipplans.FirstOrDefaultAsync(p => p.Name == "Yoga");
        plan.Should().NotBeNull();
        plan!.Price.Should().Be(300);
    }

    [Fact]
    public async Task GetPlans_WithFilter_Should_ReturnCorrectPlans()
    {
        Context.Membershipplans.AddRange(
            new MembershipPlan { Name = "Unique Cheap", Price = 333, DurationMonths = 1 },
            new MembershipPlan { Name = "Unique Expensive", Price = 9999, DurationMonths = 12 }
        );
        await Context.SaveChangesAsync();
        var plans = await Client.GetFromJsonAsync<List<MembershipPlan>>("/api/v1/membership-plans?minPrice=9000");

        plans.Should().HaveCount(1);
        plans![0].Name.Should().Be("Unique Expensive");
    }

    [Fact]
    public async Task DeletePlan_WithActiveMemberships_Should_Fail()
    {
        var plan = new MembershipPlan { Name = "Locked Plan", Price = 100, DurationMonths = 1 };
        var client = new Client { Name = "User", Email = "u@test.com", Phone = "123", Password = "p" };
        Context.AddRange(plan, client);
        await Context.SaveChangesAsync();

        Context.Memberships.Add(new Membership 
        { 
            ClientId = client.ClientId, 
            PlanId = plan.PlanId, 
            IsActive = true,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1))
        });
        await Context.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/v1/membership-plans/{plan.PlanId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var exists = await Context.Membershipplans.AnyAsync(p => p.PlanId == plan.PlanId);
        exists.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeletePlan_Unused_Should_ReturnNoContent_And_DeleteFromDb()
    {
        var plan = new MembershipPlan { Name = "Useless Plan", Price = 10, DurationMonths = 1 };
        Context.Membershipplans.Add(plan);
        await Context.SaveChangesAsync();
        var response = await Client.DeleteAsync($"/api/v1/membership-plans/{plan.PlanId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var exists = await Context.Membershipplans.AnyAsync(p => p.PlanId == plan.PlanId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetPlanById_Should_ReturnCorrectPlan()
    {
        var plan = new MembershipPlan { Name = "Specific Plan", Price = 99, DurationMonths = 1 };
        Context.Membershipplans.Add(plan);
        await Context.SaveChangesAsync();

        var result = await Client.GetFromJsonAsync<MembershipPlan>($"/api/v1/membership-plans/{plan.PlanId}");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Specific Plan");
    }
}