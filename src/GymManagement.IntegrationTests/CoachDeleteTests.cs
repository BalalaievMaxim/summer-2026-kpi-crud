using System.Net;
using FluentAssertions;
using GymManagement.Core.Entities;
using Xunit;

namespace GymManagement.IntegrationTests;

public class CoachDeleteTests : BaseIntegrationTest
{
    public CoachDeleteTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteCoach_WithFutureClassesNoEnrollments_DeletesEverything()
    {
        var coach = new Coach 
        { 
            Name = "Mike Trainer", 
            Specialization = "Boxing", 
            Email = "mike@gym.com",
            Password = "hash789"
        };
        
        var classType = new ClassType 
        { 
            Name = "Boxing", 
            Description = "Fight training" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var futureClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
            Capacity = 15
        };
        Context.Classes.Add(futureClass);
        await Context.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/coach/{coach.CoachId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        Context.ChangeTracker.Clear();
        
        var deletedClass = await Context.Classes.FindAsync(futureClass.ClassId);
        deletedClass.Should().BeNull();
        
        var deletedCoach = await Context.Coaches.FindAsync(coach.CoachId);
        deletedCoach.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCoach_WithEnrolledClients_ReturnsBadRequest()
    {
        var coach = new Coach 
        { 
            Name = "Popular Coach", 
            Specialization = "Zumba", 
            Email = "popular@gym.com",
            Password = "hash"
        };
        
        var classType = new ClassType 
        { 
            Name = "Zumba", 
            Description = "Dance" 
        };

        var client = new Client
        {
            Name = "Test Client",
            Email = "client@test.com",
            Phone = "123456789",
            Password = "pass"
        };

        Context.AddRange(coach, classType, client);
        await Context.SaveChangesAsync();

        var futureClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(3),
            EndTime = DateTime.UtcNow.AddDays(3).AddHours(1),
            Capacity = 20
        };
        Context.Classes.Add(futureClass);
        await Context.SaveChangesAsync();

        var enrollment = new Enrollment
        {
            ClientId = client.ClientId,
            ClassId = futureClass.ClassId
        };
        Context.Enrollments.Add(enrollment);
        await Context.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/coach/{coach.CoachId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        Context.ChangeTracker.Clear();
        
        var stillExists = await Context.Coaches.FindAsync(coach.CoachId);
        stillExists.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteCoach_NonExistent_ReturnsNotFound()
    {
        var response = await Client.DeleteAsync("/api/coach/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
