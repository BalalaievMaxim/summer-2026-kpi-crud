using FluentAssertions;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;

namespace GymManagement.Tests.Unit.Domain;

public class ClientTests
{
    private const string ValidName = "John Doe";
    private const string ValidEmail = "john@example.com";
    private const string ValidPhone = "+380671234567";
    private const string ValidPassword = "pass1234";
    private static readonly TestPasswordHasher Hasher = new();

    private static Client CreateClient()
        => Client.Create(ValidName, ValidEmail, ValidPhone, ValidPassword, Hasher);

    [Fact]
    public void Create_ValidData_ReturnsClient()
    {
        var client = CreateClient();

        client.Name.Value.Should().Be(ValidName);
        client.Email.Value.Should().Be(ValidEmail);
        client.Phone.Value.Should().Be(ValidPhone);
        client.MatchesPassword(ValidPassword, Hasher).Should().BeTrue();
        client.Password.Value.Should().Be("test-hash:pass1234");
        client.Id.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsInvalidClientNameError(string name)
    {
        var act = () => Client.Create(name, ValidEmail, ValidPhone, ValidPassword, Hasher);
        act.Should().Throw<InvalidClientNameError>();
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsInvalidEmailError()
    {
        var act = () => Client.Create(ValidName, "not-an-email", ValidPhone, ValidPassword, Hasher);
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void Create_InvalidPhone_ThrowsInvalidPhoneNumberError()
    {
        var act = () => Client.Create(ValidName, ValidEmail, "abc", ValidPassword, Hasher);
        act.Should().Throw<InvalidPhoneNumberError>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Create_ShortPassword_ThrowsInvalidPasswordError(string password)
    {
        var act = () => Client.Create(ValidName, ValidEmail, ValidPhone, password, Hasher);
        act.Should().Throw<InvalidPasswordError>();
    }

    [Fact]
    public void UpdateContact_ValidData_UpdatesEmailAndPhone()
    {
        var client = CreateClient();

        client.UpdateContact("new@example.com", "+380679999999");

        client.Email.Value.Should().Be("new@example.com");
        client.Phone.Value.Should().Be("+380679999999");
    }

    [Fact]
    public void UpdateContact_InvalidEmail_Throws()
    {
        var client = CreateClient();
        var act = () => client.UpdateContact("bad-email", ValidPhone);
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void UpdateName_ValidName_Updates()
    {
        var client = CreateClient();

        client.UpdateName("Jane Smith");

        client.Name.Value.Should().Be("Jane Smith");
    }

    [Fact]
    public void UpdateName_EmptyName_ThrowsInvalidClientNameError()
    {
        var client = CreateClient();
        var act = () => client.UpdateName("   ");
        act.Should().Throw<InvalidClientNameError>();
    }

    [Fact]
    public void MatchesPassword_CorrectPassword_ReturnsTrue()
    {
        var client = CreateClient();
        client.MatchesPassword(ValidPassword, Hasher).Should().BeTrue();
    }

    [Fact]
    public void MatchesPassword_WrongPassword_ReturnsFalse()
    {
        var client = CreateClient();
        client.MatchesPassword("wrongpass", Hasher).Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_ValidCurrentPassword_ChangesPassword()
    {
        var client = CreateClient();

        client.ChangePassword(ValidPassword, "newpass99", Hasher);

        client.MatchesPassword("newpass99", Hasher).Should().BeTrue();
        client.MatchesPassword(ValidPassword, Hasher).Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_WrongCurrentPassword_ThrowsInvalidCredentialsError()
    {
        var client = CreateClient();
        var act = () => client.ChangePassword("wrongpass", "newpass99", Hasher);
        act.Should().Throw<InvalidCredentialsError>();
    }

    [Fact]
    public void ChangePassword_ShortNewPassword_ThrowsInvalidPasswordError()
    {
        var client = CreateClient();
        var act = () => client.ChangePassword(ValidPassword, "abc", Hasher);
        act.Should().Throw<InvalidPasswordError>();
    }

    [Fact]
    public void Reconstitute_AssignsCorrectId()
    {
        var client = Client.Reconstitute(42, ValidName, ValidEmail, ValidPhone, ValidPassword);
        client.Id.Should().Be(42);
        client.Name.Value.Should().Be(ValidName);
    }

    [Fact]
    public void TwoClients_SameId_AreEqual()
    {
        var a = Client.Reconstitute(5, "Alice", "a@example.com", ValidPhone, ValidPassword);
        var b = Client.Reconstitute(5, "Bob", "b@example.com", ValidPhone, ValidPassword);
        a.Should().Be(b);
    }

    [Fact]
    public void TwoClients_DifferentIds_AreNotEqual()
    {
        var a = Client.Reconstitute(1, ValidName, ValidEmail, ValidPhone, ValidPassword);
        var b = Client.Reconstitute(2, ValidName, ValidEmail, ValidPhone, ValidPassword);
        a.Should().NotBe(b);
    }
}
