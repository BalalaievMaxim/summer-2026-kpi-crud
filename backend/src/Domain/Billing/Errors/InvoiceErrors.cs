using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Billing.Errors;

public sealed class InvoiceNotFoundError : DomainError
{
    public InvoiceNotFoundError(Guid id)
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
