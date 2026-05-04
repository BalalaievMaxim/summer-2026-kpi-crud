using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Infrastructure.Persistence.Entities;
using Xunit;

namespace GymManagement.Tests.Integration;

public class ClassUpdateTests : BaseIntegrationTest
{
    public ClassUpdateTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task RescheduleClass_ValidData_ReturnsUpdatedClass()
    {
        var coach = new Coach 
        { 
            Name = "Update Coach", 
            Specialization = "Fitness", 
            Email = "update@gym.com",
            Password = "hash"
        };
        
        var classType = new ClassType 
        { 
            Name = "Fitness Class", 
            Description = "Test" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var originalStart = DateTime.UtcNow.AddDays(5);
        var classEntity = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = originalStart,
            EndTime = originalStart.AddHours(1),
            Capacity = 10
        };
        Context.Classes.Add(classEntity);
        await Context.SaveChangesAsync();

        var newStart = DateTime.UtcNow.AddDays(6);
        var updateRequest = new
        {
            NewStartTime = newStart,
            NewEndTime = newStart.AddHours(1)
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/class/{classEntity.ClassId}/reschedule", 
            updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Class>();
        updated.Should().NotBeNull();
        updated!.StartTime.Should().BeCloseTo(newStart, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RescheduleClass_AlreadyStarted_ReturnsBadRequest()
    {
        var coach = new Coach 
        { 
            Name = "Past Coach", 
            Specialization = "Boxing", 
            Email = "past@gym.com",
            Password = "hash"
        };
        
        var classType = new ClassType 
        { 
            Name = "Boxing", 
            Description = "Test" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var pastStart = DateTime.UtcNow.AddHours(-2);
        var classEntity = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = pastStart,
            EndTime = pastStart.AddHours(1),
            Capacity = 10
        };
        Context.Classes.Add(classEntity);
        await Context.SaveChangesAsync();

        var updateRequest = new
        {
            NewStartTime = DateTime.UtcNow.AddDays(1),
            NewEndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/class/{classEntity.ClassId}/reschedule", 
            updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RescheduleClass_NewTimeHasConflict_ReturnsBadRequest()
    {
        var coach = new Coach 
        { 
            Name = "Busy Coach", 
            Specialization = "Yoga", 
            Email = "busy@gym.com",
            Password = "hash"
        };
        
        var classType = new ClassType 
        { 
            Name = "Yoga", 
            Description = "Test" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var fixedTime = DateTime.UtcNow.AddDays(10);

        var class1 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = fixedTime,
            EndTime = fixedTime.AddHours(1),
            Capacity = 10
        };

        var class2 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = fixedTime.AddDays(1),
            EndTime = fixedTime.AddDays(1).AddHours(1),
            Capacity = 10
        };

        Context.Classes.AddRange(class1, class2);
        await Context.SaveChangesAsync();

        var updateRequest = new
        {
            NewStartTime = fixedTime.AddMinutes(30),
            NewEndTime = fixedTime.AddHours(1).AddMinutes(30)
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/class/{class2.ClassId}/reschedule", 
            updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
