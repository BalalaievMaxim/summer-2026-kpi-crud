using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Tests.Integration;

public class ClientManagementTests(GymApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateClient_Should_Fail_When_NewEmailAlreadyExists()
    {
        var client1 = new Client { Name = "Перший клієнт", Email = "first@test.com", Phone = "1234567", Password = "pass1234" };
        var client2 = new Client { Name = "Другий клієнт", Email = "second@test.com", Phone = "7654321", Password = "pass1234" };
        Context.Clients.AddRange(client1, client2);
        await Context.SaveChangesAsync();

        var updateDto = new UpdateClientDto
        {
            Email = "second@test.com",
            Phone = "3334444"
        };

        var response = await Client.PutAsJsonAsync($"/api/v1/clients/{client1.ClientId}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already exists");
    }

    [Fact]
    public async Task DeleteClient_WithActiveEnrollments_Should_Return400()
    {
        var coach = new Coach { Name = "Іван Піддубний", Specialization = "Тренер", Email = "ivan@test.com", Password = "pass1234" };
        var classType = new ClassType { Name = "Якесь заняття" };
        Context.AddRange(coach, classType);
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

        var client = new Client { Name = "Клієнт з записами", Email = "delete@test.com", Phone = "1234567", Password = "pass1234" };
        Context.Clients.Add(client);
        await Context.SaveChangesAsync();

        var enrollment = new Enrollment { ClientId = client.ClientId, ClassId = gymClass.ClassId };
        Context.Enrollments.Add(enrollment);
        await Context.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/v1/clients/{client.ClientId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("active enrollments");

        Context.ChangeTracker.Clear();

        var clientInDb = await Context.Clients.FirstOrDefaultAsync(c => c.ClientId == client.ClientId);
        clientInDb.Should().NotBeNull();

        var enrollmentInDb = await Context.Enrollments.FirstOrDefaultAsync(e => e.EnrollmentId == enrollment.EnrollmentId);
        enrollmentInDb.Should().NotBeNull();
    }
}