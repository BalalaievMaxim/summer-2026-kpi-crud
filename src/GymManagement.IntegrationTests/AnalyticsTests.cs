using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;

namespace GymManagement.IntegrationTests;

public class AnalyticsTests(GymApiFactory factory) : BaseIntegrationTest(factory)
{
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

    [Fact]
    public async Task GetClientActivityAnalytics_Should_CorrectlyRankClients_ByEnrollmentCount()
    {
        var coach = new Coach { Name = "Іван Піддубний", Specialization = "Зальчик", Email = "ivan@test.com", Password = "pass" };
        var classType = new ClassType { Name = "Якесь заняття" };
        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var classes = new List<Class>();
        var baseTime = DateTime.UtcNow.AddDays(1);
        for (int i = 0; i < 5; i++)
        {
            classes.Add(new Class
            {
                ClassTypeId = classType.ClassTypeId,
                CoachId = coach.CoachId,
                StartTime = baseTime.AddHours(i),
                EndTime = baseTime.AddHours(i + 1),
                Capacity = 10
            });
        }
        Context.Classes.AddRange(classes);
        await Context.SaveChangesAsync();

        var clientA = new Client { Name = "Клієнт A (Top)", Email = "a@test.com", Phone = "111", Password = "p" };
        var clientB = new Client { Name = "Клієнт B (Low)", Email = "b@test.com", Phone = "222", Password = "p" };
        var clientC = new Client { Name = "Клієнт C (Top)", Email = "c@test.com", Phone = "333", Password = "p" };
        Context.Clients.AddRange(clientA, clientB, clientC);
        await Context.SaveChangesAsync();

        foreach (var c in classes)
            Context.Enrollments.Add(new Enrollment { ClientId = clientA.ClientId, ClassId = c.ClassId });

        foreach (var c in classes)
            Context.Enrollments.Add(new Enrollment { ClientId = clientC.ClientId, ClassId = c.ClassId });

        for (int i = 0; i < 3; i++)
            Context.Enrollments.Add(new Enrollment { ClientId = clientB.ClientId, ClassId = classes[i].ClassId });

        await Context.SaveChangesAsync();

        var result = await Client.GetFromJsonAsync<List<ClientActivityDto>>("/api/v1/clients/analytics/activity");

        result.Should().NotBeNull();

        var statsA = result!.First(r => r.ClientId == clientA.ClientId);
        var statsB = result!.First(r => r.ClientId == clientB.ClientId);
        var statsC = result!.First(r => r.ClientId == clientC.ClientId);

        statsA.TotalEnrollments.Should().Be(5);
        statsC.TotalEnrollments.Should().Be(5);
        statsB.TotalEnrollments.Should().Be(3);

        statsA.ClientRank.Should().Be(1);
        statsC.ClientRank.Should().Be(1);
        statsB.ClientRank.Should().Be(3);
    }
}