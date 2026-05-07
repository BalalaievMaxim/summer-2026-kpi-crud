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

public sealed class InvalidClientNameError : DomainError
{
    public InvalidClientNameError()
        : base("Client.InvalidName", "Client name cannot be empty.") { }
}

public sealed class InvalidPasswordError : DomainError
{
    public InvalidPasswordError()
        : base("Client.InvalidPassword", "Password must be at least 4 characters.") { }
}

public sealed class InvalidCredentialsError : DomainError
{
    public InvalidCredentialsError()
        : base("Client.InvalidCredentials", "Invalid email or password.") { }
}

public sealed class ClientEmailAlreadyExistsError : DomainError
{
    public ClientEmailAlreadyExistsError(string email)
        : base("Client.EmailAlreadyExists", $"A client with email '{email}' already exists.") { }
}

public sealed class ClientNotFoundError : DomainError
{
    public ClientNotFoundError(int id)
        : base("Client.NotFound", $"Client with id {id} was not found.") { }
}

public sealed class ClientHasActiveEnrollmentsError : DomainError
{
    public ClientHasActiveEnrollmentsError(int clientId)
        : base("Client.HasActiveEnrollments", $"Client {clientId} has active enrollments. Cancel them before deletion.") { }
}

public sealed class ClientHasActiveMembershipsError : DomainError
{
    public ClientHasActiveMembershipsError(int clientId)
        : base("Client.HasActiveMemberships", $"Client {clientId} has an active membership. Cancel it before deletion.") { }
}
