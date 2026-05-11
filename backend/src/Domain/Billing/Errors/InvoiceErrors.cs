using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Billing.Errors;

public sealed class InvoiceNotFoundError : DomainError
{
    public InvoiceNotFoundError(int id)
        : base("Invoice.NotFound", $"Invoice with id '{id}' was not found.") { }
}

public sealed class InvalidInvoiceError : DomainError
{
    public InvalidInvoiceError(string message)
        : base("Invoice.Invalid", message) { }
}

public sealed class InvalidInvoiceOperationError : DomainError
{
    public InvalidInvoiceOperationError(string message)
        : base("Invoice.InvalidOperation", message) { }
}

public sealed class ClientNotFoundForInvoiceError : DomainError
{
    public ClientNotFoundForInvoiceError(int clientId)
        : base("Invoice.ClientNotFound", $"Client with id {clientId} was not found.") { }
}

public sealed class MembershipPlanNotFoundForInvoiceError : DomainError
{
    public MembershipPlanNotFoundForInvoiceError(int planId)
        : base("Invoice.MembershipPlanNotFound", $"Membership Plan with id {planId} was not found.") { }
}
