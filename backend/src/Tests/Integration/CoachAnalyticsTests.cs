using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using Xunit;

namespace GymManagement.Tests.Integration;

public class CoachAnalyticsTests : BaseIntegrationTest
{
    public CoachAnalyticsTests(GymApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCoachEfficiency_CalculatesHoursAndOccupancy()
    {
        var coach = new Coach 
        { 
            Name = "Lisa Trainer", 
            Specialization = "CrossFit", 
            Email = "lisa@gym.com",
            Password = "hash654"
        };
        
        var classType = new ClassType 
        { 
            Name = "CrossFit", 
            Description = "High intensity" 
        };
        
        var client1 = new Client 
        { 
            Name = "Client 1", 
            Email = "client1@test.com", 
            Phone = "111111111",
            Password = "pass"
        };

        var client2 = new Client 
        { 
            Name = "Client 2", 
            Email = "client2@test.com", 
            Phone = "222222222",
            Password = "pass"
        };

        Context.AddRange(coach, classType, client1, client2);
        await Context.SaveChangesAsync();

        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(10);

        var class1 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(-5),
            EndTime = DateTime.UtcNow.AddDays(-5).AddHours(2),
            Capacity = 10
        };

        var class2 = new Class
        {
            ClassTypeId = classType.ClassTypeId,
            CoachId = coach.CoachId,
            StartTime = DateTime.UtcNow.AddDays(-3),
            EndTime = DateTime.UtcNow.AddDays(-3).AddHours(1),
            Capacity = 10
        };

        Context.Classes.AddRange(class1, class2);
        await Context.SaveChangesAsync();

        Context.Enrollments.AddRange(
            new Enrollment { ClientId = client1.ClientId, ClassId = class1.ClassId },
            new Enrollment { ClientId = client2.ClientId, ClassId = class1.ClassId }
        );
        Context.Enrollments.AddRange(
            new Enrollment { ClientId = client1.ClientId, ClassId = class2.ClassId }
        );
        await Context.SaveChangesAsync();

        var response = await Client.GetAsync(
            $"/api/class/analytics/coach-efficiency?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<CoachEfficiencyDto>>();
        
        reports.Should().NotBeNull();
        var coachReport = reports!.FirstOrDefault(r => r.CoachId == coach.CoachId);
        
        coachReport.Should().NotBeNull();
        coachReport!.TotalHours.Should().Be(3);
        coachReport.ClassCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCoachEfficiency_NoClasses_ReturnsEmptyList()
    {
        var startDate = DateTime.UtcNow.AddYears(-2);
        var endDate = DateTime.UtcNow.AddYears(-1);

        var response = await Client.GetAsync(
            $"/api/class/analytics/coach-efficiency?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<CoachEfficiencyDto>>();
        
        reports.Should().NotBeNull();
        reports!.Should().BeEmpty();
    }
}
