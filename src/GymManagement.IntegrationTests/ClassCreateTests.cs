using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Core.Entities;
using Xunit;

namespace GymManagement.IntegrationTests;

public class ClassCreateTests : BaseIntegrationTest
{
    public ClassCreateTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateClass_ValidData_ReturnsCreatedClass()
    {
        var coach = new Coach 
        { 
            Name = "John Trainer", 
            Specialization = "Yoga", 
            Email = "john@gym.com",
            Password = "hash123"
        };
        
        var classType = new ClassType 
        { 
            Name = "Hatha Yoga", 
            Description = "Relaxing yoga class" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var request = new
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 10
        };

        var response = await Client.PostAsJsonAsync("/api/class", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdClass = await response.Content.ReadFromJsonAsync<Class>();
        createdClass.Should().NotBeNull();
        createdClass!.CoachId.Should().Be(coach.CoachId);
        createdClass.Capacity.Should().Be(10);
    }

    [Fact]
    public async Task CreateClass_CoachHasConflict_ReturnsBadRequest()
    {
        var coach = new Coach 
        { 
            Name = "Jane Trainer", 
            Specialization = "Pilates", 
            Email = "jane@gym.com",
            Password = "hash456"
        };
        
        var classType = new ClassType 
        { 
            Name = "Pilates", 
            Description = "Core workout" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2);
        var endTime = startTime.AddHours(1);

        var existingClass = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = startTime,
            EndTime = endTime,
            Capacity = 10
        };
        Context.Classes.Add(existingClass);
        await Context.SaveChangesAsync();

        var request = new
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = startTime.AddMinutes(30),
            EndTime = endTime.AddMinutes(30),
            Capacity = 8
        };

        var response = await Client.PostAsJsonAsync("/api/class", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateClass_InvalidTimeRange_ReturnsBadRequest()
    {
        var coach = new Coach 
        { 
            Name = "Test Coach", 
            Specialization = "Test", 
            Email = "test@gym.com",
            Password = "hash"
        };
        
        var classType = new ClassType 
        { 
            Name = "Test Class", 
            Description = "Test" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(-30);

        var request = new
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = startTime,
            EndTime = endTime,
            Capacity = 10
        };

        var response = await Client.PostAsJsonAsync("/api/class", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
