using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Core.Entities;
using Xunit;

namespace GymManagement.IntegrationTests;

public class ClassSelectTests : BaseIntegrationTest
{
    public ClassSelectTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetScheduleForDate_ReturnsClassesSortedByTime()
    {
        var coach = new Coach 
        { 
            Name = "Sarah Trainer", 
            Specialization = "Zumba", 
            Email = "sarah@gym.com",
            Password = "hash321"
        };
        
        var classType = new ClassType 
        { 
            Name = "Zumba", 
            Description = "Dance fitness" 
        };

        Context.AddRange(coach, classType);
        await Context.SaveChangesAsync();

        var targetDate = DateTime.UtcNow.AddDays(3).Date;

        var class1 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = targetDate.AddHours(10),
            EndTime = targetDate.AddHours(11),
            Capacity = 20
        };

        var class2 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = targetDate.AddHours(8),
            EndTime = targetDate.AddHours(9),
            Capacity = 15
        };

        var class3 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = targetDate.AddHours(14),
            EndTime = targetDate.AddHours(15),
            Capacity = 25
        };

        Context.Classes.AddRange(class1, class2, class3);
        await Context.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/class/schedule/{targetDate:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var schedule = await response.Content.ReadFromJsonAsync<List<Class>>();
        
        schedule.Should().NotBeNull();
        schedule!.Should().HaveCount(3);
        schedule[0].StartTime.Hour.Should().Be(8);
        schedule[1].StartTime.Hour.Should().Be(10);
        schedule[2].StartTime.Hour.Should().Be(14);
    }

    [Fact]
    public async Task GetCoachesBySpecialization_ReturnsFilteredCoaches()
    {
        var coach1 = new Coach 
        { 
            Name = "Yoga Master", 
            Specialization = "Yoga", 
            Email = "yoga1@gym.com",
            Password = "hash"
        };

        var coach2 = new Coach 
        { 
            Name = "Yoga Expert", 
            Specialization = "Yoga", 
            Email = "yoga2@gym.com",
            Password = "hash"
        };

        var coach3 = new Coach 
        { 
            Name = "Boxing Pro", 
            Specialization = "Boxing", 
            Email = "boxing@gym.com",
            Password = "hash"
        };

        Context.Coaches.AddRange(coach1, coach2, coach3);
        await Context.SaveChangesAsync();

        var response = await Client.GetAsync("/api/coach/specialization/Yoga");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var yogaCoaches = await response.Content.ReadFromJsonAsync<List<Coach>>();
        
        yogaCoaches.Should().NotBeNull();
        yogaCoaches!.Should().HaveCount(2);
        yogaCoaches.Should().AllSatisfy(c => c.Specialization.Should().Be("Yoga"));
    }

    [Fact]
    public async Task GetScheduleForDate_NoClasses_ReturnsEmptyList()
    {
        var emptyDate = DateTime.UtcNow.AddYears(1).Date;

        var response = await Client.GetAsync($"/api/class/schedule/{emptyDate:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var schedule = await response.Content.ReadFromJsonAsync<List<Class>>();
        
        schedule.Should().NotBeNull();
        schedule!.Should().BeEmpty();
    }
}
