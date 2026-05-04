using FluentAssertions;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;

namespace GymManagement.Tests.Unit.Domain;

public class CoachTests
{
    [Fact]
    public void Create_ValidData_ReturnsCoach()
    {
        var coach = Coach.Create(1, "Maria", "Smith", "Yoga", 42);

        coach.Id.Should().Be(1);
        coach.FirstName.Should().Be("Maria");
        coach.LastName.Should().Be("Smith");
        coach.Specialization.Value.Should().Be("Yoga");
        coach.UserId.Should().Be(42);
    }

    [Fact]
    public void Create_EmptySpecialization_Throws()
    {
        var act = () => Coach.Create(1, "Maria", "Smith", "  ", 42);
        act.Should().Throw<InvalidSpecializationError>();
    }

    [Fact]
    public void UpdateSpecialization_ValidValue_Updates()
    {
        var coach = Coach.Create(1, "Maria", "Smith", "Yoga", 42);

        coach.UpdateSpecialization("CrossFit");

        coach.Specialization.Value.Should().Be("CrossFit");
    }

    [Fact]
    public void UpdateSpecialization_TooLong_Throws()
    {
        var coach = Coach.Create(1, "Maria", "Smith", "Yoga", 42);

        var act = () => coach.UpdateSpecialization(new string('x', 101));
        act.Should().Throw<InvalidSpecializationError>();
    }

    [Fact]
    public void TwoCoaches_SameId_AreEqual()
    {
        var a = Coach.Create(3, "A", "A", "Yoga", 1);
        var b = Coach.Create(3, "B", "B", "CrossFit", 2);
        a.Should().Be(b);
    }
}
