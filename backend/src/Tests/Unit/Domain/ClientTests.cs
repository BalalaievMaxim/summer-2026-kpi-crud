using FluentAssertions;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;

namespace GymManagement.Tests.Unit.Domain;

public class ClientTests
{
    [Fact]
    public void Create_ValidData_ReturnsClient()
    {
        var client = Client.Create(1, "John", "Doe", "john@example.com", "+380671234567");

        client.Id.Should().Be(1);
        client.FirstName.Should().Be("John");
        client.LastName.Should().Be("Doe");
        client.Email.Value.Should().Be("john@example.com");
        client.Phone.Value.Should().Be("+380671234567");
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsInvalidEmailError()
    {
        var act = () => Client.Create(1, "John", "Doe", "not-an-email", "+380671234567");
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void Create_InvalidPhone_ThrowsInvalidPhoneNumberError()
    {
        var act = () => Client.Create(1, "John", "Doe", "john@example.com", "abc");
        act.Should().Throw<InvalidPhoneNumberError>();
    }

    [Fact]
    public void UpdateContact_ValidData_UpdatesEmailAndPhone()
    {
        var client = Client.Create(1, "John", "Doe", "old@example.com", "+380671111111");

        client.UpdateContact("new@example.com", "+380672222222");

        client.Email.Value.Should().Be("new@example.com");
        client.Phone.Value.Should().Be("+380672222222");
    }

    [Fact]
    public void UpdateContact_InvalidEmail_Throws()
    {
        var client = Client.Create(1, "John", "Doe", "john@example.com", "+380671234567");

        var act = () => client.UpdateContact("bad", "+380671234567");
        act.Should().Throw<InvalidEmailError>();
    }

    [Fact]
    public void TwoClients_SameId_AreEqual()
    {
        var a = Client.Create(5, "John", "Doe", "a@example.com", "+380671234567");
        var b = Client.Create(5, "Jane", "Smith", "b@example.com", "+380679999999");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoClients_DifferentIds_AreNotEqual()
    {
        var a = Client.Create(1, "John", "Doe", "a@example.com", "+380671234567");
        var b = Client.Create(2, "John", "Doe", "a@example.com", "+380671234567");
        a.Should().NotBe(b);
    }
}
