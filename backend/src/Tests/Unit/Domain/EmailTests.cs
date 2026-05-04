using FluentAssertions;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Tests.Unit.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user+tag@domain.org")]
    public void Create_ValidEmail_ReturnsLowercasedEmail(string input)
    {
        var email = Email.Create(input);
        email.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("missing@")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidEmail_ThrowsInvalidEmailError(string input)
    {
        var act = () => Email.Create(input);
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void TwoEmails_SameValue_AreEqual()
    {
        var a = Email.Create("test@example.com");
        var b = Email.Create("TEST@EXAMPLE.COM");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoEmails_DifferentValues_AreNotEqual()
    {
        var a = Email.Create("a@example.com");
        var b = Email.Create("b@example.com");
        a.Should().NotBe(b);
    }
}
