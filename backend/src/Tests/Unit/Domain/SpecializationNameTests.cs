using FluentAssertions;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Tests.Unit.Domain;

public class SpecializationNameTests
{
    [Fact]
    public void Create_ValidName_ReturnsTrimmedValue()
    {
        var spec = SpecializationName.Create("  Yoga  ");
        spec.Value.Should().Be("Yoga");
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        var act = () => SpecializationName.Create("   ");
        act.Should().Throw<InvalidSpecializationError>();
    }

    [Fact]
    public void Create_NameTooLong_Throws()
    {
        var act = () => SpecializationName.Create(new string('x', 101));
        act.Should().Throw<InvalidSpecializationError>();
    }

    [Fact]
    public void Create_ExactlyMaxLength_Succeeds()
    {
        var act = () => SpecializationName.Create(new string('x', 100));
        act.Should().NotThrow();
    }
}
