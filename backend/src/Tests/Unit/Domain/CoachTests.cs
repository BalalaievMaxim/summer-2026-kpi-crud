using FluentAssertions;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using InvalidEmailError = GymManagement.Domain.Clients.Errors.InvalidEmailError;

namespace GymManagement.Tests.Unit.Domain;

public class CoachTests
{
    private const string ValidName = "Maria Smith";
    private const string ValidEmail = "maria@gym.com";
    private const string ValidSpec = "Yoga";
    private const string ValidPassword = "pass1234";

    [Fact]
    public void Create_ValidData_ReturnsCoach()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);

        coach.Name.Value.Should().Be(ValidName);
        coach.Email.Value.Should().Be(ValidEmail);
        coach.Specialization.Value.Should().Be(ValidSpec);
        coach.Password.Value.Should().Be(ValidPassword);
        coach.Id.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsInvalidCoachNameError(string name)
    {
        var act = () => Coach.Create(name, ValidEmail, ValidSpec, ValidPassword);
        act.Should().Throw<InvalidCoachNameError>();
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsInvalidEmailError()
    {
        var act = () => Coach.Create(ValidName, "bad-email", ValidSpec, ValidPassword);
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void Create_EmptySpecialization_ThrowsInvalidSpecializationError()
    {
        var act = () => Coach.Create(ValidName, ValidEmail, "  ", ValidPassword);
        act.Should().Throw<InvalidSpecializationError>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Create_ShortPassword_ThrowsInvalidPasswordError(string password)
    {
        var act = () => Coach.Create(ValidName, ValidEmail, ValidSpec, password);
        act.Should().Throw<InvalidPasswordError>();
    }

    [Fact]
    public void UpdateSpecialization_ValidValue_Updates()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);

        coach.UpdateSpecialization("CrossFit");

        coach.Specialization.Value.Should().Be("CrossFit");
    }

    [Fact]
    public void UpdateSpecialization_TooLong_ThrowsInvalidSpecializationError()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        var act = () => coach.UpdateSpecialization(new string('x', 101));
        act.Should().Throw<InvalidSpecializationError>();
    }

    [Fact]
    public void UpdateName_ValidName_Updates()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);

        coach.UpdateName("Ivan Petrenko");

        coach.Name.Value.Should().Be("Ivan Petrenko");
    }

    [Fact]
    public void UpdateName_EmptyName_ThrowsInvalidCoachNameError()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        var act = () => coach.UpdateName("   ");
        act.Should().Throw<InvalidCoachNameError>();
    }

    [Fact]
    public void UpdateEmail_ValidEmail_Updates()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);

        coach.UpdateEmail("newemail@gym.com");

        coach.Email.Value.Should().Be("newemail@gym.com");
    }

    [Fact]
    public void UpdateEmail_InvalidEmail_ThrowsInvalidEmailError()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        var act = () => coach.UpdateEmail("not-an-email");
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void MatchesPassword_CorrectPassword_ReturnsTrue()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        coach.MatchesPassword(ValidPassword).Should().BeTrue();
    }

    [Fact]
    public void MatchesPassword_WrongPassword_ReturnsFalse()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        coach.MatchesPassword("wrongpass").Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_ValidCurrentPassword_ChangesPassword()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);

        coach.ChangePassword(ValidPassword, "newpass99");

        coach.MatchesPassword("newpass99").Should().BeTrue();
        coach.MatchesPassword(ValidPassword).Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_WrongCurrentPassword_ThrowsInvalidCoachCredentialsError()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        var act = () => coach.ChangePassword("wrongpass", "newpass99");
        act.Should().Throw<InvalidCoachCredentialsError>();
    }

    [Fact]
    public void ChangePassword_ShortNewPassword_ThrowsInvalidPasswordError()
    {
        var coach = Coach.Create(ValidName, ValidEmail, ValidSpec, ValidPassword);
        var act = () => coach.ChangePassword(ValidPassword, "abc");
        act.Should().Throw<InvalidPasswordError>();
    }

    [Fact]
    public void Reconstitute_AssignsCorrectId()
    {
        var coach = Coach.Reconstitute(7, ValidName, ValidEmail, ValidSpec, ValidPassword);
        coach.Id.Should().Be(7);
        coach.Name.Value.Should().Be(ValidName);
    }

    [Fact]
    public void TwoCoaches_SameId_AreEqual()
    {
        var a = Coach.Reconstitute(3, "A", "a@gym.com", "Yoga", ValidPassword);
        var b = Coach.Reconstitute(3, "B", "b@gym.com", "CrossFit", ValidPassword);
        a.Should().Be(b);
    }
}
