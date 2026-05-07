using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Coaches;

public sealed class Coach : AggregateRoot<int>
{
    public PersonName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public SpecializationName Specialization { get; private set; } = null!;
    public Password Password { get; private set; } = null!;

    private Coach() { }

    private Coach(int id, PersonName name, Email email, SpecializationName specialization, Password password) : base(id)
    {
        Name = name;
        Email = email;
        Specialization = specialization;
        Password = password;
    }

    public static Coach Create(string name, string email, string specialization, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCoachNameError();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            throw new InvalidPasswordError();

        return new Coach(
            id: 0,
            name: PersonName.Create(name),
            email: Email.Create(email),
            specialization: SpecializationName.Create(specialization),
            password: Password.Create(password));
    }

    public static Coach Reconstitute(int id, string name, string email, string specialization, string password)
        => new(id, PersonName.Reconstitute(name), Email.Create(email), SpecializationName.Create(specialization), Password.Reconstitute(password));

    public void UpdateSpecialization(string specialization)
        => Specialization = SpecializationName.Create(specialization);

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCoachNameError();
        Name = PersonName.Create(name);
    }

    public void UpdateEmail(string email)
        => Email = Email.Create(email);

    public bool MatchesPassword(string password) => Password.Matches(password);

    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!MatchesPassword(currentPassword))
            throw new InvalidCoachCredentialsError();

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            throw new InvalidPasswordError();

        Password = Password.Create(newPassword);
    }
}
