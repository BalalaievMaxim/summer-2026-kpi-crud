using FluentAssertions;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Tests.Unit.Domain;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+380671234567")]
    [InlineData("0671234567")]
    [InlineData("1234567")]
    public void Create_ValidPhone_ReturnsPhoneNumber(string input)
    {
        var phone = PhoneNumber.Create(input);
        phone.Value.Should().Be(input);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+1-800-FLOWERS")]
    public void Create_InvalidPhone_ThrowsInvalidPhoneNumberError(string input)
    {
        var act = () => PhoneNumber.Create(input);
        act.Should().Throw<InvalidPhoneNumberError>();
    }

    [Fact]
    public void TwoPhones_SameValue_AreEqual()
    {
        var a = PhoneNumber.Create("+380671234567");
        var b = PhoneNumber.Create("+380671234567");
        a.Should().Be(b);
    }
}
