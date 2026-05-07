using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Clients;

public sealed class Client : AggregateRoot<int>
{
    public PersonName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;
    public Password Password { get; private set; } = null!;

    private Client() { }

    private Client(int id, PersonName name, Email email, PhoneNumber phone, Password password) : base(id)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Password = password;
    }

    public static Client Create(string name, string email, string phone, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidClientNameError();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            throw new InvalidPasswordError();

        return new Client(
            id: 0,
            name: PersonName.Create(name),
            email: Email.Create(email),
            phone: PhoneNumber.Create(phone),
            password: Password.Create(password));
    }

    public static Client Reconstitute(int id, string name, string email, string phone, string password)
        => new(id, PersonName.Reconstitute(name), Email.Create(email), PhoneNumber.Create(phone), Password.Reconstitute(password));

    public void UpdateContact(string email, string phone)
    {
        Email = Email.Create(email);
        Phone = PhoneNumber.Create(phone);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidClientNameError();
        Name = PersonName.Create(name);
    }

    public bool MatchesPassword(string password) => Password.Matches(password);

    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!MatchesPassword(currentPassword))
            throw new InvalidCredentialsError();

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            throw new InvalidPasswordError();

        Password = Password.Create(newPassword);
    }
}
