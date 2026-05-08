namespace GymManagement.Domain.Billing;

public enum PaymentStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Online
}
