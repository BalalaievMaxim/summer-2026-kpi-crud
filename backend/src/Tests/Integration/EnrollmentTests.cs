using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Tests.Integration;

public class EnrollmentTests(GymApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task EnrollClient_Should_Succeed_When_MembershipIsActive_And_ClassHasCapacity()
    {
        var coach = new Coach { Name = "Іван Піддубний", Specialization = "Зальчик", Email = "ivan@test.com", Password = "p" };
        var classType = new ClassType { Name = "Якесь заняття" };
        var plan = new MembershipPlan { Name = "Золотий план", Price = 100, DurationMonths = 1 };
        Context.AddRange(coach, classType, plan);
        await Context.SaveChangesAsync();

        var gymClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 10
        };
        Context.Classes.Add(gymClass);

        var client = new Client { Name = "Клієнт ОК", Email = "ok@test.com", Phone = "111", Password = "p" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        await SeedActiveMembershipAsync(client.ClientId, plan.PlanId);

        var dto = new CreateEnrollmentDto { ClientId = client.ClientId, ClassId = gymClass.ClassId };

        var response = await Client.PostAsJsonAsync("/api/v1/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var enrollment = Context.Enrollments.FirstOrDefault(e => e.ClientId == client.ClientId && e.ClassId == gymClass.ClassId);
        enrollment.Should().NotBeNull();
    }

    [Fact]
    public async Task EnrollClient_Should_Fail_When_ClassCapacityIsReached()
    {
        var coach = new Coach { Name = "Василь Піддубний", Specialization = "Пілатес", Email = "vas@test.com", Password = "p" };
        var classType = new ClassType { Name = "Пілатес" };
        var plan = new MembershipPlan { Name = "Срібний план", Price = 100, DurationMonths = 1 };
        Context.AddRange(coach, classType, plan);
        await Context.SaveChangesAsync();

        var gymClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 1
        };
        Context.Classes.Add(gymClass);
        await Context.SaveChangesAsync();

        var client1 = new Client { Name = "Клєінт1", Email = "c1@test.com", Phone = "111", Password = "p" };
        var client2 = new Client { Name = "Клієнт2", Email = "c2@test.com", Phone = "222", Password = "p" };
        Context.Clients.AddRange(client1, client2);
        await Context.SaveChangesAsync();

        await SeedActiveMembershipAsync(client1.ClientId, plan.PlanId);
        await SeedActiveMembershipAsync(client2.ClientId, plan.PlanId);

        Context.Enrollments.Add(new Enrollment { ClientId = client1.ClientId, ClassId = gymClass.ClassId });
        await Context.SaveChangesAsync();

        var dto = new CreateEnrollmentDto { ClientId = client2.ClientId, ClassId = gymClass.ClassId };

        var response = await Client.PostAsJsonAsync("/api/v1/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Class.Full");
    }

    [Fact]
    public async Task EnrollClient_Should_Fail_When_ClientHasNoActiveMembership()
    {
        var coach = new Coach { Name = "Дмитро Піддубний", Specialization = "Стронгмен", Email = "dm@test.com", Password = "p" };
        var classType = new ClassType { Name = "Стронг" };
        var plan = new MembershipPlan { Name = "Бронзовий план", Price = 100, DurationMonths = 1 };
        Context.AddRange(coach, classType, plan);
        await Context.SaveChangesAsync();

        var gymClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 10
        };
        Context.Classes.Add(gymClass);

        var client = new Client { Name = "Неактивний клієнт", Email = "no@test.com", Phone = "000", Password = "p" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        await SeedPendingMembershipAsync(client.ClientId, plan.PlanId);

        var dto = new CreateEnrollmentDto { ClientId = client.ClientId, ClassId = gymClass.ClassId };

        var response = await Client.PostAsJsonAsync("/api/v1/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("active membership");
    }

    [Fact]
    public async Task EnrollClient_Should_Fail_When_MembershipIsExpired()
    {
        var coach = new Coach { Name = "Ігор Піддубний", Specialization = "Паверліфтер", Email = "igor@test.com", Password = "p" };
        var classType = new ClassType { Name = "Паверліфтинг" };
        var plan = new MembershipPlan { Name = "Діамантовий план", Price = 100, DurationMonths = 1 };
        Context.AddRange(coach, classType, plan);
        await Context.SaveChangesAsync();

        var gymClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 10
        };
        Context.Classes.Add(gymClass);

        var client = new Client { Name = "Просрочений клієнт", Email = "expired@test.com", Phone = "999", Password = "p" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        await SeedActiveMembershipAsync(client.ClientId, plan.PlanId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var membership = await Context.Memberships.FirstAsync(m => m.ClientId == client.ClientId);
        membership.EndDate = today.AddDays(-1);
        await Context.SaveChangesAsync();

        var dto = new CreateEnrollmentDto { ClientId = client.ClientId, ClassId = gymClass.ClassId };

        var response = await Client.PostAsJsonAsync("/api/v1/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("active membership");
    }

    [Fact]
    public async Task EnrollClient_Should_Fail_When_ClientIsAlreadyEnrolledInSameClass()
    {
        var coach = new Coach { Name = "Володимир Піддубний", Specialization = "Калістеніка", Email = "vol@test.com", Password = "p" };
        var classType = new ClassType { Name = "Калістеніка" };
        var plan = new MembershipPlan { Name = "Платиновий план", Price = 100, DurationMonths = 1 };
        Context.AddRange(coach, classType, plan);
        await Context.SaveChangesAsync();

        var gymClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 10
        };
        Context.Classes.Add(gymClass);

        var client = new Client { Name = "Наполегливий клієнт", Email = "duplicate@test.com", Phone = "888", Password = "p" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        await SeedActiveMembershipAsync(client.ClientId, plan.PlanId);

        Context.Enrollments.Add(new Enrollment { ClientId = client.ClientId, ClassId = gymClass.ClassId });
        await Context.SaveChangesAsync();

        var dto = new CreateEnrollmentDto { ClientId = client.ClientId, ClassId = gymClass.ClassId };

        var response = await Client.PostAsJsonAsync("/api/v1/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already enrolled");
    }
}
