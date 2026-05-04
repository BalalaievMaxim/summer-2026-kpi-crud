using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Clients;

public sealed class Client : AggregateRoot<int>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;

    private Client() { }

    private Client(int id, string firstName, string lastName, Email email, PhoneNumber phone)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
    }

    public static Client Create(int id, string firstName, string lastName, string email, string phone)
    {
        var emailVo = Email.Create(email);
        var phoneVo = PhoneNumber.Create(phone);
        return new Client(id, firstName, lastName, emailVo, phoneVo);
    }

    public void UpdateContact(string email, string phone)
    {
        Email = Email.Create(email);
        Phone = PhoneNumber.Create(phone);
    }
}
