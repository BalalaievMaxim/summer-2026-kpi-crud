using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Clients.Errors;

public sealed class InvalidEmailError : DomainError
{
    public InvalidEmailError(string value)
        : base("Client.InvalidEmail", $"'{value}' is not a valid email address.") { }
}

public sealed class InvalidPhoneNumberError : DomainError
{
    public InvalidPhoneNumberError(string value)
        : base("Client.InvalidPhone", $"'{value}' is not a valid phone number.") { }
}

public sealed class ClientNotFoundError : DomainError
{
    public ClientNotFoundError(int id)
        : base("Client.NotFound", $"Client with id {id} was not found.") { }
}
