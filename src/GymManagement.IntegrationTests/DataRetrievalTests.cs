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


    [Fact]
    public async Task SearchClients_Should_ReturnCorrectResults_When_SearchingByNameOrEmail()
    {
        var client1 = new Client { Name = "Іван Петров", Email = "evan@test.com", Phone = "111", Password = "p" };
        var client2 = new Client { Name = "Петро Іванов", Email = "petro@test.com", Phone = "222", Password = "p" };
        var client3 = new Client { Name = "Олег Сидоров", Email = "oleg@search.com", Phone = "333", Password = "p" };
        var client4 = new Client { Name = "Хтось іще", Email = "unique_email@test.com", Phone = "444", Password = "p" };

        Context.Clients.AddRange(client1, client2, client3, client4);
        await Context.SaveChangesAsync();

        var resultName = await Client.GetFromJsonAsync<List<Client>>("/api/v1/clients/search?searchTerm=Петр");

        var resultEmail = await Client.GetFromJsonAsync<List<Client>>("/api/v1/clients/search?searchTerm=unique");

        resultName.Should().NotBeNull();
        resultName.Should().Contain(c => c.ClientId == client1.ClientId);
        resultName.Should().Contain(c => c.ClientId == client2.ClientId);
        resultName.Should().NotContain(c => c.ClientId == client3.ClientId);

        resultEmail.Should().HaveCount(1);
        resultEmail![0].Name.Should().Be("Хтось іще");
    }

    [Fact]
    public async Task GetClientClassHistory_Should_ReturnAllPastEnrollments()
    {
        var coach = new Coach { Name = "Іван Піддубний", Specialization = "Тренер", Email = "ivan@test.com", Password = "p" };
        var classType = new ClassType { Name = "Якесь заняття" };
        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var client = new Client { Name = "Клієнт", Email = "anyclient@test.com", Phone = "555", Password = "p" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        var pastClass1 = new Class { ClassTypeId = classType.ClassTypeId, CoachId = coach.CoachId, StartTime = DateTime.UtcNow.AddDays(-10), EndTime = DateTime.UtcNow.AddDays(-10).AddHours(1), Capacity = 10 };
        var pastClass2 = new Class { ClassTypeId = classType.ClassTypeId, CoachId = coach.CoachId, StartTime = DateTime.UtcNow.AddDays(-5), EndTime = DateTime.UtcNow.AddDays(-5).AddHours(1), Capacity = 10 };
        var futureClass = new Class { ClassTypeId = classType.ClassTypeId, CoachId = coach.CoachId, StartTime = DateTime.UtcNow.AddDays(5), EndTime = DateTime.UtcNow.AddDays(5).AddHours(1), Capacity = 10 };

        Context.Classes.AddRange(pastClass1, pastClass2, futureClass);
        await Context.SaveChangesAsync();

        Context.Enrollments.AddRange(
            new Enrollment { ClientId = client.ClientId, ClassId = pastClass1.ClassId },
            new Enrollment { ClientId = client.ClientId, ClassId = pastClass2.ClassId },
            new Enrollment { ClientId = client.ClientId, ClassId = futureClass.ClassId }
        );
        await Context.SaveChangesAsync();

        var resultClient = await Client.GetFromJsonAsync<Client>($"/api/v1/clients/{client.ClientId}/history");

        resultClient.Should().NotBeNull();
        resultClient!.Enrollments.Should().NotBeNull();
        resultClient.Enrollments.Should().HaveCount(3);

        resultClient.Enrollments.Select(e => e.ClassId).Should().Contain(new[]
        {
            pastClass1.ClassId,
            pastClass2.ClassId,
            futureClass.ClassId
        });
    }
}